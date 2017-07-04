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
    /// discovery mechanisms. 
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
        
        protected abstract Task<IEnumerable<Address>> GetAliveNodesAsync();

        protected abstract Task RegisterNodeAsync(MemberEntry entry);

        protected abstract Task MarkAsAliveAsync(MemberEntry entry);

        protected abstract void SendJoinSignal();

        #endregion

        protected readonly Cluster Cluster;
        protected readonly MemberEntry Entry;
        protected readonly ILoggingAdapter Log;

        private readonly ClusterDiscoverySettings settings;
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
                        Log.Error(cause, "Failed to obtain a distributed lock for actor system [{0}] after {1} retries. Closing.", Context.System.Name, retries);
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

            refreshTask = Context.System.Scheduler
                .ScheduleTellRepeatedlyCancelable(settings.AliveInterval, settings.AliveInterval, Self,
                    new Alive(Entry.Address), ActorRefs.NoSender);

            Become(Ready);
            return true;
        }

        private void Ready()
        {
            ReceiveAsync<Alive>(alive => this.MarkAsAliveAsync(Entry));
        }

        protected override void PreStart()
        {
            SendJoinSignal();
            base.PreStart();
        }

        protected override void PostStop()
        {
            refreshTask?.Cancel();
            base.PostStop();
        }
    }
}