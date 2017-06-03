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

        public interface IMessage { }
        public sealed class Lock : IMessage
        {
            public static readonly Lock Instance = new Lock();
            private Lock() { }
        }
        public sealed class IAmAlive : IMessage
        {
            public IAmAlive(Address selfAddress)
            {
                SelfAddress = selfAddress;
            }

            public Address SelfAddress { get; }

        }

        #endregion

        #region abstract messages

        protected abstract Task<bool> LockAsync(string systemName);

        protected abstract Task UnlockAsync(string systemName);

        protected abstract Task<IEnumerable<Address>> GetAliveNodesAsync(string systemName);

        protected abstract Task RegisterNodeAsync(MemberEntry entry);

        protected abstract Task MarkAsAliveAsync(MemberEntry entry);

        #endregion
        
        private readonly Cluster cluster;
        private readonly ClusterDiscoverySettings settings;
        private readonly ILoggingAdapter log;
        private readonly MemberEntry entry;

        private ICancelable refreshTask;

        protected DiscoveryService(ClusterDiscoverySettings settings)
        {
            this.cluster = Cluster.Get(Context.System);
            this.log = Context.GetLogger();
            this.settings = settings;
            this.entry = new MemberEntry(Context.System.Name, cluster.SelfAddress, cluster.SelfRoles);

            Initializing();
        }

        private void Initializing()
        {
            var retries = 0;
            ReceiveAsync<Lock>(async _ =>
            {
                var system = Context.System;
                var locked = await LockAsync(system.Name);
                if (locked)
                {
                    try
                    {
                        var nodes = (await GetAliveNodesAsync(system.Name)).ToArray();
                        if (nodes.Length == 0)
                            cluster.JoinSeedNodes(new[] { entry.Address });
                        else
                            cluster.JoinSeedNodes(nodes);

                        await RegisterNodeAsync(entry);
                        await MarkAsAliveAsync(entry);
                        
                        refreshTask = Context.System.Scheduler
                            .ScheduleTellRepeatedlyCancelable(settings.AliveInterval, settings.AliveInterval, Self, new IAmAlive(entry.Address), ActorRefs.NoSender);

                        Become(Alive);
                    }
                    catch (Exception cause)
                    {
                        log.Error(cause, "Failed to obtain a distributed lock for actor system [{0}] after {1} retries. Closing.", Context.System.Name, retries);
                        Context.Stop(Self);
                    }
                    finally
                    {
                        await UnlockAsync(system.Name);
                    }
                }
                else
                {
                    retries++;
                    log.Warning("Failed to obtain a distributed lock for actor system [{0}]. Remaining retries: {1}. Retry in [{2}]", Context.System.Name, retries, settings.LockRetryInterval);
                    Context.System.Scheduler.ScheduleTellOnce(settings.LockRetryInterval, Self, Lock.Instance, ActorRefs.NoSender);
                }
            });
        }

        private void Alive()
        {
            ReceiveAsync<IAmAlive>(async alive =>
            {
                await MarkAsAliveAsync(entry);
            });
        }

        protected override void PreStart()
        {
            Self.Tell(Lock.Instance);
            base.PreStart();
        }

        protected override void PostStop()
        {
            refreshTask?.Cancel();
            base.PostStop();
        }
    }
}