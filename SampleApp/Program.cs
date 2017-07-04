using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
