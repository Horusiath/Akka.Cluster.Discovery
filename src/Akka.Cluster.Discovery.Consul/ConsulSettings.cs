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
        }

        public ConsulSettings()
        {
            ListenerUrl = new Uri("http://127.0.0.1:8500");
            Datacenter = null;
            Token = null;
            WaitTime = null;
        }

        public ConsulSettings(Uri listenerUrl,
            string datacenter,
            string token,
            TimeSpan? waitTime,
            TimeSpan aliveInterval, 
            TimeSpan aliveTimeout, 
            TimeSpan refreshInterval,
            int joinRetries, 
            TimeSpan lockRetryInterval) 
            : base(aliveInterval, aliveTimeout, refreshInterval, joinRetries, lockRetryInterval)
        {
            ListenerUrl = listenerUrl;
            Datacenter = datacenter;
            Token = token;
            WaitTime = waitTime;
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
    }
}