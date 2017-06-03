using Akka.Actor;
using Akka.Cluster.Discovery.LockFile;

namespace Akka.Cluster.Discovery
{
    public sealed class ClusterDiscovery : IExtension 
    {
        public IActorRef OracleRef { get; }

        public ClusterDiscovery(ExtendedActorSystem system, ClusterDiscoverySettings settings)
        {
        }
    }

    internal sealed class ClusterDiscoveryProvider : ExtensionIdProvider<ClusterDiscovery>
    {
        private readonly ClusterDiscoverySettings settings;

        public ClusterDiscoveryProvider(ClusterDiscoverySettings settings)
        {
            this.settings = settings;
        }

        public override ClusterDiscovery CreateExtension(ExtendedActorSystem system)
        {
            return new ClusterDiscovery(system, settings);
        }
    }
}