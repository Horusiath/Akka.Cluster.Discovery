using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Discovery;
using Akka.Configuration;
using Consul;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }
        
        private static async Task RunAsync()
        {
            var config = ConfigurationFactory.ParseString(@"
	            akka {
		            actor.provider = cluster
                    remote.dot-netty.tcp {
                        hostname = ""127.0.0.1""
                        port = 0
                    }
		            cluster.discovery {
			            provider = akka.cluster.discovery.consul
			            consul {
				            listener-url = ""http://127.0.0.1:8500""
		                    refresh-interval = 10s
                            # restart consul client every 5 minutes
                            restart-interval = 5m
			            }
		            }
	            }");

            using (var system = ActorSystem.Create("sample", config))
            {
                await ClusterDiscovery.JoinAsync(system);

                system.ActorOf(Props.Create<SampleActor>());

                Console.WriteLine("Press enter to shutdown the current node...");
                Console.ReadLine();

                var cluster = Cluster.Get(system);
                await cluster.LeaveAsync();

                Console.WriteLine("Node terminated. Press enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
