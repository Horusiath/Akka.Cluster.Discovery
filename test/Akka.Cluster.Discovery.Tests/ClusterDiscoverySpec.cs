using System;
using Akka.Cluster.TestKit;
using Akka.Remote.TestKit;

namespace Akka.Cluster.Discovery.Tests
{
    public abstract class ClusterDiscoveryNodeConfig : MultiNodeConfig
    {
        protected ClusterDiscoveryNodeConfig()
        {

        }
    }

    public abstract class ClusterDiscoverySpec : MultiNodeClusterSpec
    {
        protected ClusterDiscoverySpec(MultiNodeConfig config, Type type) : base(config, type)
        {
        }
    }
}