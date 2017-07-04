using System;
using Akka.Configuration;

namespace Akka.Cluster.Discovery.ServiceFabric
{
    public sealed class ServiceFabricSettings : LocklessClusterDiscoverySettings
    {
        public ServiceFabricSettings()
        {
        }

        public ServiceFabricSettings(TimeSpan aliveInterval, TimeSpan aliveTimeout, int joinRetries, TimeSpan turnPeriod, int maxTurns) : base(aliveInterval, aliveTimeout, joinRetries, turnPeriod, maxTurns)
        {
        }

        public ServiceFabricSettings(Config config) : base(config)
        {
        }
    }
}