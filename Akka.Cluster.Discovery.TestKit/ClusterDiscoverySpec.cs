using System;
using Akka.Actor;
using Akka.Cluster.TestKit;
using Akka.Configuration;
using Akka.Remote.TestKit;

namespace Akka.Cluster.Discovery.TestKit
{
    public class ClusterDiscoverySpecConfig : MultiNodeConfig
    {
        public RoleName First { get; }
        public RoleName Second { get; }
        public RoleName Third { get; }

        public ClusterDiscoverySpecConfig()
        {
            First = new RoleName("first");
            Second = new RoleName("second");
            Third = new RoleName("third");

            CommonConfig = ConfigurationFactory.ParseString(@"
                akka.loglevel = DEBUG
                akka.actor.provider = cluster
            ");

            TestTransport = true;
        }
    }

    public abstract class ClusterDiscoverySpec : MultiNodeClusterSpec
    {
        public RoleName First { get; }
        public RoleName Second { get; }
        public RoleName Third { get; }

        protected ClusterDiscoverySpec(ClusterDiscoverySpecConfig config) 
            : base(config)
        {
            First = config.First;
            Second = config.Second;
            Third = config.Third;
        }

        public void DiscoveryService_must_be_able_to_initialize_the_cluster()
        {
            RunOn(() =>
            {
                ClusterDiscovery.Run(Sys);

                AwaitClusterUp(First);
                AssertLeader(First);
            }, First);
        }

        public void DiscoveryService_must_be_able_to_join_nodes_to_existing_cluster()
        {
            RunOn(() =>
            {
                ClusterDiscovery.Run(Sys);

                AwaitClusterUp(Second, Third);
            }, Second, Third);
        }
    }
}