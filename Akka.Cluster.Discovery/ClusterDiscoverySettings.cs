using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery
{
    public class ClusterDiscoverySettings
    {
        public TimeSpan LockRetryInterval { get; }
        
        public TimeSpan AliveInterval { get; }

        public int LockRetries { get; }

        public ClusterDiscoverySettings() : this(
            aliveInterval: TimeSpan.FromSeconds(5),
            lockRetryInterval: TimeSpan.FromSeconds(2),
            lockRetries: 5)
        { }

        public ClusterDiscoverySettings(TimeSpan aliveInterval, TimeSpan lockRetryInterval, int lockRetries)
        {
            AliveInterval = aliveInterval;
            LockRetries = lockRetries;
            LockRetryInterval = lockRetryInterval;
        }

        public ClusterDiscoverySettings(Config config) : this(
            aliveInterval: config.GetTimeSpan("alive-interval"),
            lockRetryInterval: config.GetTimeSpan("lock-retry-interval"),
            lockRetries: config.GetInt("lock-retries"))
        { }
    }
}