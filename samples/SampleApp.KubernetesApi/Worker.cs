using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Discovery;
using Akka.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleApp.KubernetesApi
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var myPodIp = GetLocalIPAddress();

            var config = ConfigurationFactory.ParseString(@"
	            akka {
		            actor.provider = cluster
                    remote.dot-netty.tcp {
                        hostname = """ + myPodIp + @"""
                        port = 2551 
                    }
                    cluster.roles = [sample,demo]
		            cluster.discovery {
			            provider = akka.cluster.discovery.k8s
			            k8s {
		                    refresh-interval = 10s
                            namespace = ""default""
                            label-selector = ""akka-cluster=sample,env=Development""
                        }
		            }
	            }");

            using var system = ActorSystem.Create("sample", config);
            await ClusterDiscovery.JoinAsync(system);

            system.ActorOf(Props.Create<SampleActor>());

            try
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"My address: {myPodIp}");

                    var state = Cluster.Get(system).State;

                    foreach (var member in state.Members)
                    {
                        sb.AppendLine($"    Member: {member.Address} = {member.Status}, Seen by = {state.SeenBy.Contains(member.Address)} [{string.Join(", ", member.Roles)}]");                        
                    }
                    logger.LogInformation("Status:\r\n" + sb.ToString());

                    await Task.Delay(10000, stoppingToken);
                }
            }
            finally
            {
                var cluster = Cluster.Get(system);
                await cluster.LeaveAsync();
            }
        }
    }
}
