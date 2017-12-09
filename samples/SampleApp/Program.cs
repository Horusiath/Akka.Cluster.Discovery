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
            //var cancel = new CancellationTokenSource();
            //RunAsync(cancel.Token).Wait();

            Run();
        }

        private static async Task RunAsync(CancellationToken token)
        {
            using (var consul = new ConsulClient())
            {
                var id = $"mycluster@127.0.0.1:12000";
                var s = new AgentServiceRegistration
                {
                    ID = id,
                    Name = "mycluster",
                    Tags = new string[0],
                    Address = "127.0.0.1",
                    Port = 12001,
                    Check = new AgentServiceCheck
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                        TTL = TimeSpan.FromSeconds(15)
                    }
                };
                await consul.Agent.ServiceRegister(s);

                var t = new Thread(_ =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            consul.Agent.PassTTL("service:" + id, "", token).Wait();
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                        }
                    })
                    { IsBackground = true };
                t.Start();

                Console.Write("Started...");
                Console.ReadLine();
            }
        }

        private static void Run()
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
		                    refresh-interval = 1m
			            }
		            }
	            }");

            using (var system = ActorSystem.Create("sample", config))
            {
                ClusterDiscovery.Join(system);

                system.ActorOf(Props.Create<SampleActor>());
                Console.ReadLine();
            }
        }
    }
}
