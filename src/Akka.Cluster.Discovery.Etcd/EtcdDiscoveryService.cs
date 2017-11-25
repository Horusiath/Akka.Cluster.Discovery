using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using netcd;
using netcd.Serialization;

namespace Akka.Cluster.Discovery.Etcd
{
    public class EtcdDiscoveryService : LockingDiscoveryService
    {
        private readonly netcd.EtcdClient etcd;

        public EtcdDiscoveryService(EtcdSettings settings) : base(settings)
        {
        }

        protected override async Task<IEnumerable<Address>> GetAliveNodesAsync()
        {
            

            throw new System.NotImplementedException();
        }

        protected override async Task RegisterNodeAsync(MemberEntry node)
        {

            throw new System.NotImplementedException();
        }

        protected override async Task MarkAsAliveAsync(MemberEntry node)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<bool> LockAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task UnlockAsync(string key)
        {
            throw new System.NotImplementedException();
        }
    }
}