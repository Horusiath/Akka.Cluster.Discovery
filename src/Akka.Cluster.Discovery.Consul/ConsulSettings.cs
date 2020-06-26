#region copyright
// -----------------------------------------------------------------------
// <copyright file="ConsulSettings.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Configuration;

namespace Akka.Cluster.Discovery.Consul
{
    public class ConsulSettings : LockingClusterDiscoverySettings
    {
        public ConsulSettings(Config config) : base(config)
        {
            ListenerUrl = new Uri(config.GetString("listener-url", "http://127.0.0.1:8500"));
            Datacenter = config.GetString("datacenter");
            Token = config.GetString("token");
            WaitTime = !config.HasPath("wait-time") ? default(TimeSpan?) : config.GetTimeSpan("wait-time");
            RestartInterval = !config.HasPath("restart-interval") ? default(TimeSpan?) : config.GetTimeSpan("restart-interval");

            var serviceCheckTtl = config.GetTimeSpan("service-check-ttl", new TimeSpan(this.AliveInterval.Ticks * 3));
            if (serviceCheckTtl < AliveInterval || serviceCheckTtl > AliveTimeout) throw new ArgumentException("`akka.cluster.discovery.consul.service-check-ttl` must greater than `akka.cluster.discovery.consul.alive-interval` and less than `akka.cluster.discovery.consul.alive-timeout`");
            
            ServiceCheckTtl = serviceCheckTtl;
        }

        public ConsulSettings() : base()
        {
            ListenerUrl = new Uri("http://127.0.0.1:8500");
            Datacenter = null;
            Token = null;
            WaitTime = null;
            ServiceCheckTtl = new TimeSpan(this.AliveInterval.Ticks * 3);
            RestartInterval = null;
        }

        public ConsulSettings(Uri listenerUrl,
            string datacenter,
            string token,
            TimeSpan? waitTime,
            TimeSpan aliveInterval, 
            TimeSpan aliveTimeout, 
            TimeSpan refreshInterval,
            int joinRetries, 
            TimeSpan lockRetryInterval,
            TimeSpan serviceCheckTtl,
            TimeSpan? restartInterval) 
            : base(aliveInterval, aliveTimeout, refreshInterval, joinRetries, lockRetryInterval)
        {
            if (serviceCheckTtl < AliveInterval || serviceCheckTtl > AliveTimeout) throw new ArgumentException("serviceCheckTtl must greater than aliveInterval and less than aliveTimeout", nameof(serviceCheckTtl));

            ListenerUrl = listenerUrl;
            Datacenter = datacenter;
            Token = token;
            WaitTime = waitTime;
            ServiceCheckTtl = serviceCheckTtl;
            RestartInterval = restartInterval;
        }

        /// <summary>
        /// URL address on with Consul listener service can be found.
        /// </summary>
        public Uri ListenerUrl { get; }

        /// <summary>
        /// Consul datacenter.
        /// </summary>
        public string Datacenter { get; }

        /// <summary>
        /// Consul token.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// (Optional) Timeout for consul client connection requests.
        /// </summary>
        public TimeSpan? WaitTime { get; }

        /// <summary>
        /// A timeout configured for consul to mark a time to live given for a node before it will be 
        /// marked as unhealthy. Must be greater than <see cref="ClusterDiscoverySettings.AliveInterval"/>
        /// and less than <see cref="ClusterDiscoverySettings.AliveTimeout"/>.
        /// </summary>
        public TimeSpan ServiceCheckTtl { get; }
        
        /// <summary>
        /// An interval in which consul client will be triggered for periodic restarts.
        /// If not provided or 0, client will never be restarted. Default value: null. 
        /// </summary>
        public TimeSpan? RestartInterval { get; }
    }
}