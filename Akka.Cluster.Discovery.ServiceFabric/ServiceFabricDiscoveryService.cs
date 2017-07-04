using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery.ServiceFabric
{
    public sealed class ServiceFabricDiscoveryService: LocklessDiscoveryService
    {
        public ServiceFabricDiscoveryService(Config config) : this(new ServiceFabricSettings(config))
        {
        }

        public ServiceFabricDiscoveryService(ServiceFabricSettings settings) : base(settings)
        {
        }

        protected override Task<IEnumerable<Address>> GetAliveNodesAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task RegisterNodeAsync(MemberEntry node)
        {
            throw new System.NotImplementedException();
        }

        protected override Task MarkAsAliveAsync(MemberEntry node)
        {
            throw new System.NotImplementedException();
        }
    }
}
