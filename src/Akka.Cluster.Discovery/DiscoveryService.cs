using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// An abstract actor, that provides basic protocol for 3rd party  cluster 
    /// discovery mechanisms. In practice all of its implementations are deriving
    /// from either:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="LockingDiscoveryService"/></term>
    ///         <description>
    ///             Used by services that allows for distributed lock acquisition for coordination.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="LocklessDiscoveryService"/></term>
    ///         <description>
    ///             For services using randomized turn-based cluster seed registery.
    ///         </description>
    ///     </item>
    /// </list>
    /// </summary>
    public abstract class DiscoveryService : ReceiveActor
    {
        #region messages

        /// <summary>
        /// Common interface for are signals defined for classes extending 
        /// <see cref="DiscoveryService"/>.
        /// </summary>
        public interface IMessage { }

        /// <summary>
        /// Signal send to discovery service to trigger to cluster 
        /// join/initialization procedure.
        /// </summary>
        public sealed class Join : IMessage
        {
            public static readonly Join Instance = new Join();
            private Join() { }
        }

        /// <summary>
        /// Signal send to discovery service to request underlying provider
        /// for a fresh state of dicovered services. That state will be then
        /// converged with a last known existing state of the Akka cluster -
        /// in case of conflict, external service dicovery provider will have
        /// priority, causing all non-registered nodes from the local cluster
        /// state to be downed.
        /// </summary>
        public sealed class Reconcile : IMessage
        {
            public static readonly Reconcile Instance = new Reconcile();
            private Reconcile() { }
        }

        /// <summary>
        /// Message send periodically (in intervals defined by 
        /// <see cref="ClusterDiscoverySettings.AliveInterval"/>) to trigger TTL
        /// refresh.
        /// </summary>
        public sealed class Alive : IMessage
        {
            public Alive(Address selfAddress)
            {
                SelfAddress = selfAddress;
            }

            public Address SelfAddress { get; }

        }

        #endregion

        #region abstract messages
        
        /// <summary>
        /// Asynchronously tries to retrieve list of other cluster nodes currently
        /// known as alive. If there are no alive nodes or cluster was not established
        /// yet, it returns an empty collections.
        /// </summary>
        /// <returns></returns>
        protected abstract Task<IEnumerable<Address>> GetAliveNodesAsync();

        /// <summary>
        /// Registers provided <paramref name="node"/> inside of current service.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract Task RegisterNodeAsync(MemberEntry node);

        /// <summary>
        /// Used for health checks. Triggers healt check heartbeat, marking current
        /// <paramref name="node"/> as alive.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract Task MarkAsAliveAsync(MemberEntry node);

        /// <summary>
        /// Triggers <see cref="Join"/> request signal to be send. It may be immediate
        /// or set some time in the future.
        /// </summary>
        protected abstract void SendJoinSignal();

        #endregion

        /// <summary>
        /// Current cluster extension.
        /// </summary>
        protected readonly Cluster Cluster;

        /// <summary>
        /// Member entry representing data about current cluster node.
        /// </summary>
        protected readonly MemberEntry Entry;

        /// <summary>
        /// Akka.NET logger.
        /// </summary>
        protected readonly ILoggingAdapter Log;

        private readonly ClusterDiscoverySettings settings;
        private ICancelable aliveTask;
        private ICancelable refreshTask;

        protected DiscoveryService(ClusterDiscoverySettings settings)
        {
            this.Cluster = Cluster.Get(Context.System);
            this.Log = Context.GetLogger();
            this.settings = settings;
            this.Entry = new MemberEntry(Context.System.Name, Cluster.SelfAddress, Cluster.SelfRoles);

            var retries = settings.JoinRetries;
            ReceiveAsync<Join>(async _ =>
            {
                retries--;
                try
                {
                    var joined = await TryJoinAsync();
                    if (!joined)
                        SendJoinSignal();
                }
                catch (Exception cause)
                {
                    if (retries > 0)
                    {
                        SendJoinSignal();
                    }
                    else
                    {
                        Log.Error(cause, "Failed to obtain a distributed lock for actor system [{0}] after {1} retries. Closing.", Context.System.Name, settings.JoinRetries);
                        Context.Stop(Self);
                    }
                }
            });
        }

        protected virtual async Task<bool> TryJoinAsync()
        {
            var nodes = (await GetAliveNodesAsync()).ToArray();
            if (nodes.Length == 0)
                Cluster.JoinSeedNodes(new[] {Entry.Address});
            else
                Cluster.JoinSeedNodes(nodes);

            await RegisterNodeAsync(Entry);
            await MarkAsAliveAsync(Entry);

            aliveTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(settings.AliveInterval, settings.AliveInterval, Self,
                    new Alive(Entry.Address), ActorRefs.NoSender);

            if (settings.RefreshInterval != TimeSpan.Zero)
            {
                Log.Debug("Setting up reconciliation task with interval {0}", settings.RefreshInterval);
                refreshTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(settings.RefreshInterval, settings.RefreshInterval, Self,
                    Reconcile.Instance, ActorRefs.NoSender);
            }

            Become(Ready);
            return true;
        }

        protected virtual void Ready()
        {
            ReceiveAsync<Alive>(alive => this.MarkAsAliveAsync(Entry));
            ReceiveAsync<Reconcile>(_ => this.ReconcileClusterStateAsync());
        }

        private async Task ReconcileClusterStateAsync()
        {
            ImmutableHashSet<Address> provided;
            try
            {
                provided = (await GetAliveNodesAsync()).ToImmutableHashSet();
            }
            catch (Exception e)
            {
                Log.Error(e, "Couldn't retrieve collection of nodes from external service. Is current node unreachable? Shutting down...");
                Cluster.Down(Cluster.SelfAddress);
                return;
            }

            var current = Cluster.State.Members.Union(Cluster.State.Unreachable).Select(m => m.Address).ToImmutableHashSet();
            
            if (!provided.SetEquals(current))
            {
                if (Log.IsInfoEnabled)
                {
                    Log.Info("Detected difference between set of nodes received from the discovery service [{0}] and the one provided by the cluster [{1}]",
                        string.Join(", ", provided), string.Join(", ", current));
                }

                // down all nodes not confirmed by service discovery provider
                foreach (var node in current.Except(provided))
                {
                    Cluster.Down(node);
                }
            }
        }

        protected override void PreStart()
        {
            SendJoinSignal();
            base.PreStart();
        }

        protected override void PostStop()
        {
            aliveTask?.Cancel();
            refreshTask?.Cancel();
            base.PostStop();
        }
    }
}