#region copyright
// -----------------------------------------------------------------------
// <copyright file="LockingDiscoveryService.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// A specialization of a core <see cref="DiscoveryService"/>, making use of abilities
    /// to establish distributed locks within third-party services (if they provide such an
    /// option). Distributed locks are less risky and doesn't bring tendency to come up with
    /// races.
    /// 
    /// For situations where 3rd party cluster discovery service relies on provider that
    /// doesn't ensure any lock, please use <see cref="LocklessDiscoveryService"/>.
    /// </summary>
    public abstract class LockingDiscoveryService : DiscoveryService
    {
        private readonly LockingClusterDiscoverySettings settings;

        protected LockingDiscoveryService(LockingClusterDiscoverySettings settings) : base(settings)
        {
            this.settings = settings;
        }

        #region abstract members

        /// <summary>
        /// Tries to acquire a lock. If failed it will retry a several times in intervals 
        /// specified by <see cref="LockingClusterDiscoverySettings.LockRetryInterval"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract Task<bool> LockAsync(string key);

        /// <summary>
        /// Releases previously acquired lock.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract Task UnlockAsync(string key);

        #endregion

        protected override void SendJoinSignal()
        {
            Self.Tell(Join.Instance);
        }

        protected override async Task<bool> TryJoinAsync()
        {
            var key = Context.System.Name;
            var locked = await LockAsync(key);
            if (locked)
            {
                try
                {
                    return await base.TryJoinAsync();
                }
                finally
                {
                    await UnlockAsync(key);
                }
            }
            else
            {
                Log.Warning("Failed to obtain a distributed lock for actor system [{0}]. Retry in [{1}]", key, settings.LockRetryInterval);
                Context.System.Scheduler.ScheduleTellOnce(settings.LockRetryInterval, Self, Join.Instance, ActorRefs.NoSender);
                return false;
            }
        }
    }

    public class LockingClusterDiscoverySettings : ClusterDiscoverySettings
    {
        /// <summary>
        /// A default value of <see cref="LockRetryInterval"/>: 250ms.
        /// </summary>
        public static readonly TimeSpan DefaultLockRetryInterval = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// In case if <see cref="LockingDiscoveryService"/> won't be able to acquire the lock,
        /// it will retry to do it again, max up to the number of times described by 
        /// <see cref="ClusterDiscoverySettings.JoinRetries"/> setting value.
        /// Default: <see cref="DefaultLockRetryInterval"/>.
        /// </summary>
        public TimeSpan LockRetryInterval { get; }

        public LockingClusterDiscoverySettings()
            : base()
        {
            LockRetryInterval = DefaultLockRetryInterval;
        }

        public LockingClusterDiscoverySettings(
            TimeSpan aliveInterval, 
            TimeSpan aliveTimeout, 
            TimeSpan refreshInterval,
            int joinRetries, 
            TimeSpan lockRetryInterval) 
            : base(aliveInterval, aliveTimeout, refreshInterval, joinRetries)
        {
            if (lockRetryInterval == TimeSpan.Zero) throw new ArgumentException("lock-retry-interval cannot be 0", nameof(lockRetryInterval));

            LockRetryInterval = lockRetryInterval;
        }

        public LockingClusterDiscoverySettings(Config config) : base(config)
        {
            var lockRetryInterval = config.GetTimeSpan("lock-retry-interval", DefaultLockRetryInterval);
            if (lockRetryInterval == TimeSpan.Zero) throw new ArgumentException("lock-retry-interval cannot be 0", nameof(lockRetryInterval));

            LockRetryInterval = lockRetryInterval;
        }
    }
}