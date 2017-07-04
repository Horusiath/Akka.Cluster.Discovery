using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// An extension build on top of Akka.NET <see cref="ActorSystem"/>, that enables 
    /// establishing cluster without having explicitly provided list of seed nodes.
    /// Those are supplied via third-party services (such as Consul or Azure Service Fabric)
    /// and managed by them.
    /// 
    /// Keep in mind, that this does NOT mean that cluster membership protocol is now
    /// controlled by external services. They are only responsible for keeping actual
    /// list of alive seed nodes.
    /// </summary>
    public sealed class ClusterDiscovery : IExtension
    {
        /// <summary>
        /// Default cluster discovery plugin configuration. 
        /// </summary>
        public static Config DefaultConfig =>
            ConfigurationFactory.FromResource<ClusterDiscovery>("Akka.Cluster.Discovery.reference.conf");

        /// <summary>
        /// Starts a cluster using configured third-party discovery serivce to supply initial
        /// list of seed nodes. If no alive nodes where detected, current node will set itself
        /// as a cluster seed.
        /// 
        /// Configuration is provided from `akka.cluster.discovery` config path.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static ClusterDiscovery Run(ActorSystem system) => system
            .WithExtension<ClusterDiscovery, ClusterDiscoveryProvider>();
        
        public IActorRef DiscoveryService { get; }

        public ClusterDiscovery(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(DefaultConfig);

            var config = system.Settings.Config.GetConfig("akka.cluster.discovery");
            var providerConfig = system.Settings.Config.GetConfig(config.GetString("provider"));
            var providerType = Type.GetType(providerConfig.GetString("class"), throwOnError: true);
            var name = config.GetString("provider-name");

            if (!typeof(ActorBase).IsAssignableFrom(providerType))
                throw new ArgumentException($"Cluster discovery provider of type [{providerType}] must be an actor.");

            DiscoveryService = CreateDiscoveryService(system, providerType, providerConfig, name);
        }

        private IActorRef CreateDiscoveryService(ExtendedActorSystem system, Type type, Config config, string name)
        {
            try
            {
                return system.SystemActorOf(Props.Create(type, config), name);
            }
            catch (Exception)
            {
                return system.SystemActorOf(Props.Create(type), name);
            }
        }
    }

    internal sealed class ClusterDiscoveryProvider : ExtensionIdProvider<ClusterDiscovery>
    {
        public ClusterDiscoveryProvider()
        {
        }

        public override ClusterDiscovery CreateExtension(ExtendedActorSystem system)
        {
            return new ClusterDiscovery(system);
        }
    }
}