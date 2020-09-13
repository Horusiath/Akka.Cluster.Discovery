using System;
using Akka.Configuration;
using k8s;

namespace Akka.Cluster.Discovery.KubernetesApi
{
    public class KubernetesSettings : LocklessClusterDiscoverySettings
    {
        public KubernetesSettings(Config config) : base(config)
        {
            this.KubernetesNamespace = config.GetString("namespace")?.ToLowerInvariant();
            this.LabelSelector = config.GetString("label-selector");
        }

        public KubernetesSettings(string kubernetesNamespace, string labelSelector) 
            : base()
        {
            this.KubernetesNamespace = kubernetesNamespace;
            this.LabelSelector = labelSelector;
        }

        public KubernetesSettings(
            string kubernetesNamespace,
            string labelSelector,
            TimeSpan aliveInterval,
            TimeSpan aliveTimeout,
            TimeSpan refreshInterval,
            int joinRetries,
            TimeSpan turnPeriod,
            int maxTurns
            )
            : base(aliveInterval, aliveTimeout, refreshInterval, joinRetries, turnPeriod, maxTurns)
        {
            this.KubernetesNamespace = kubernetesNamespace;
            this.LabelSelector = labelSelector;
        }

        public string KubernetesNamespace { get; }
        public string LabelSelector { get; } // $"app in (smartcare.api), version in ({_version})"
    }
}