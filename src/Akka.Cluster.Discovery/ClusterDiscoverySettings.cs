using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery
{
    public abstract class ClusterDiscoverySettings
    { 
        /// <summary>
        /// Default value of a <see cref="AliveInterval"/>: 5 seconds.
        /// </summary>
        public static readonly TimeSpan DefaultAliveInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Default value of <see cref="AliveTimeout"/>: 1 minute.
        /// </summary>
        public static readonly TimeSpan DefaultAliveTimeout = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Default value of <see cref="RefreshInterval"/>: 1 minute.
        /// </summary>
        public static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Default value of <see cref="JoinRetries"/>: 3.
        /// </summary>
        public static readonly int DefaultJoinRetries = 3;

        /// <summary>
        /// Time interval in which a alive signal will be send by a discovery service
        /// to fit the external service TTL expectations. 
        /// Default: <see cref="DefaultAliveInterval"/>.
        /// </summary>
        public TimeSpan AliveInterval { get; }

        /// <summary>
        /// Time to live given for a discovery service to be correctly acknowledged as
        /// alive by external monitoring service. It must be higher than 
        /// <see cref="AliveInterval"/>. 
        /// Default: <see cref="DefaultAliveTimeout"/>.
        /// </summary>
        public TimeSpan AliveTimeout { get; }

        /// <summary>
        /// Interval in which current cluster node will reach for a discovery service
        /// to retrieve data about registered node updates. Nodes, that have been detected
        /// as "lost" from service discovery provider, will be downed. 
        /// Default: <see cref="DefaultRefreshInterval"/>.
        /// </summary>
        public TimeSpan RefreshInterval { get; }

        /// <summary>
        /// Maximum number of retries given for a discovery service to register itself
        /// inside 3rd party provider before hitting hard failure. 
        /// Default: <see cref="DefaultJoinRetries"/>.
        /// </summary>
        public int JoinRetries { get; }

        protected ClusterDiscoverySettings() : this(
            aliveInterval: DefaultAliveInterval,
            aliveTimeout: DefaultAliveTimeout,
            refreshInterval: DefaultRefreshInterval,
            joinRetries: DefaultJoinRetries)
        { }

        protected ClusterDiscoverySettings(
            TimeSpan aliveInterval,
            TimeSpan aliveTimeout,
            TimeSpan refreshInterval,
            int joinRetries)
        {
            AliveInterval = aliveInterval;
            AliveTimeout = aliveTimeout;
            RefreshInterval = refreshInterval;
            JoinRetries = joinRetries;
        }

        protected ClusterDiscoverySettings(Config config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            AliveInterval = config.GetTimeSpan("alive-interval", DefaultAliveInterval);
            AliveTimeout = config.GetTimeSpan("alive-timeout", DefaultAliveTimeout);
            RefreshInterval = config.GetTimeSpan("refresh-interval", DefaultRefreshInterval);
            JoinRetries = config.GetInt("join-retries", DefaultJoinRetries);
        }
    }
}