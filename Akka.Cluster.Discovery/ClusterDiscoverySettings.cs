using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery
{
    public abstract class ClusterDiscoverySettings
    {
        /// <summary>
        /// Time interval in which a alive signal will be send by a discovery service
        /// to fit the external service TTL expectations.
        /// </summary>
        public TimeSpan AliveInterval { get; }

        /// <summary>
        /// Time to live given for a discovery service to be correctly acknowledged as
        /// alive by external monitoring service. It must be higher than 
        /// <see cref="AliveInterval"/>.
        /// </summary>
        public TimeSpan AliveTimeout { get; }

        /// <summary>
        /// Maximum number of retries given for a discovery service to register itself
        /// inside 3rd party provider before hitting hard failure.
        /// </summary>
        public int JoinRetries { get; }

        protected ClusterDiscoverySettings() : this(
            aliveInterval: TimeSpan.FromSeconds(5),
            aliveTimeout: TimeSpan.FromMinutes(1),
            joinRetries: 3)
        { }

        protected ClusterDiscoverySettings(
            TimeSpan aliveInterval,
            TimeSpan aliveTimeout,
            int joinRetries)
        {
            AliveInterval = aliveInterval;
            AliveTimeout = aliveTimeout;
            JoinRetries = joinRetries;
        }

        protected ClusterDiscoverySettings(Config config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            AliveInterval = config.GetTimeSpan("alive-interval", TimeSpan.FromSeconds(5));
            AliveTimeout = config.GetTimeSpan("alive-timeout", TimeSpan.FromMinutes(1));
            JoinRetries = config.GetInt("join-retries", 3);
        }
    }
}