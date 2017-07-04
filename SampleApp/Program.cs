using System;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Discovery;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("system"))
            {
                ClusterDiscovery.Run(system);
                Cluster.Get(system).RegisterOnMemberUp(() =>
                {
                    Console.WriteLine("Up and running");
                });

                Console.ReadLine();
            }
        }
    }
}
