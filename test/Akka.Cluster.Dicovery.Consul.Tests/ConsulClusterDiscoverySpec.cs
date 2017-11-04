using System;
using Akka.Cluster.Discovery;
using Akka.Cluster.TestKit;
using Akka.Configuration;
using Akka.Remote.TestKit;

namespace Akka.Cluster.Dicovery.Consul.Tests
{
    public sealed class ClusterDiscoverySpecConfig : MultiNodeConfig
    {
        public RoleName First { get; }
        public RoleName Second { get; }
        public RoleName Third { get; }

        public ClusterDiscoverySpecConfig()
        {
            First = Role("first");
            Second = Role("second");
            Third = Role("third");

            var config = ConfigurationFactory.ParseString(@"
	            akka {
		            actor.provider = cluster
		            cluster.discovery {
			            provider = akka.cluster.discovery.consul
			            consul {
				            listener-url = ""http://127.0.0.1:8500""
			            }
		            }
	            }
            ");

            CommonConfig = config;
        }
    }

    public class ConsulClusterDiscoverySpec : MultiNodeClusterSpec
    {
        protected ClusterDiscoverySpecConfig Configuration;
        protected readonly RoleName[] AllRoles;

        public ConsulClusterDiscoverySpec(ClusterDiscoverySpecConfig config, Type type) : base(config, type)
        {
            Configuration = config;
            AllRoles = new[] { Configuration.First, Configuration.Second, Configuration.Third };
        }

        [MultiNodeFact]
        public void ClusterDiscoveryTests()
        {
            Nodes_should_establish_cluster_using_external_service_discovery_provider();
            Nodes_which_have_disappeared_from_external_service_discovery_provider_should_be_downed();
        }

        private void Nodes_should_establish_cluster_using_external_service_discovery_provider()
        {
            RunOn(() =>
            {
                ClusterDiscovery.Run(Sys);
            }, AllRoles);

            AwaitClusterUp(AllRoles);

            EnterBarrier("after-1");
        }

        private void Nodes_which_have_disappeared_from_external_service_discovery_provider_should_be_downed()
        {
            RunOn(() =>
            {

            });
        }
    }
}