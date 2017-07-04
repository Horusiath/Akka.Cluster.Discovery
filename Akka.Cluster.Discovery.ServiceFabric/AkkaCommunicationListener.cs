using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Akka.Cluster.Discovery.ServiceFabric
{
    public class AkkaCommunicationListener : ICommunicationListener
    {
        private readonly ActorSystem system;
        private readonly Akka.Cluster.Cluster cluster;

        public AkkaCommunicationListener(ActorSystem system)
        {
            this.system = system;
            this.cluster = Cluster.Get(system);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return cluster.LeaveAsync(cancellationToken);
        }

        public void Abort()
        {
            cluster.LeaveAsync().Wait();
        }
    }
}