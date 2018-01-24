using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;

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
        /// Returns a <see cref="ClusterDiscovery"/> extension initialized for provided actor system.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static ClusterDiscovery Get(ActorSystem system) => 
            system.WithExtension<ClusterDiscovery, ClusterDiscoveryProvider>();

        /// <summary>
        /// Starts a cluster using configured third-party discovery serivce to supply initial
        /// list of seed nodes. If no alive nodes where detected, current node will set itself
        /// as a cluster seed. This is fire-and-forget method. It means, that while the cluster
        /// join procedure will be initialized, it won't block until the finish. If this is not
        /// your case, use <see cref="JoinAsync"/> or <see cref="Cluster.RegisterOnMemberUp"/>.
        /// 
        /// Configuration is provided from `akka.cluster.discovery` config path.
        /// </summary>
        /// <returns></returns>
        public static void Join(ActorSystem system)
        {
            var plugin = Get(system);
            plugin.DiscoveryService.Tell(Discovery.DiscoveryService.Init.Instance);
        }

        /// <summary>
        /// Starts a cluster using configured third-party discovery serivce to supply initial
        /// list of seed nodes. If no alive nodes where detected, current node will set itself
        /// as a cluster seed. This method will return a task, which either completes or fails,
        /// when cluster join procedure finishes.
        /// 
        /// Configuration is provided from `akka.cluster.discovery` config path.
        /// </summary>
        /// <returns></returns>
        public static Task JoinAsync(ActorSystem system, CancellationToken token = default(CancellationToken))
        {
            var promise = new TaskCompletionSource<ClusterDiscovery>();
            var cluster = Cluster.Get(system);
            var plugin = Get(system);

            cluster.RegisterOnMemberUp(() => promise.TrySetResult(plugin));
            cluster.RegisterOnMemberRemoved(() => promise.TrySetException(new ClusterJoinFailedException($"Node has not managed to join the cluster using service discovery")));

            plugin.DiscoveryService.Tell(Discovery.DiscoveryService.Init.Instance);

            return promise.Task.WithCancellation(token);
        }

        /// <summary>
        /// An actor inheriting from <see cref="Discovery.DiscoveryService"/> that is able to work
        /// with external service.
        /// </summary>
        public IActorRef DiscoveryService { get; }

        /// <summary>
        /// Creates new instance of a <see cref="ClusterDiscovery"/> extension. Extension config
        /// is supplied from `akka.cluster.discovery` HOCON path. 
        /// 
        /// Invoking this constructor triggers provided <paramref name="system"/> to initialize 
        /// cluster joining procedure.
        /// </summary>
        /// <param name="system"></param>
        public ClusterDiscovery(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(DefaultConfig);

            var config = system.Settings.Config.GetConfig("akka.cluster.discovery");
            var providerConfig = system.Settings.Config.GetConfig(config.GetString("provider"));
            var providerType = Type.GetType(providerConfig.GetString("class"), throwOnError: true);
            var dispatcher = providerConfig.GetString("dispatcher", Dispatch.Dispatchers.DefaultDispatcherId);
            var name = config.GetString("provider-name");

            if (!typeof(ActorBase).IsAssignableFrom(providerType))
                throw new ArgumentException($"Cluster discovery provider of type [{providerType}] must be an actor.");

            DiscoveryService = CreateDiscoveryService(system, providerType, providerConfig, dispatcher, name);
        }

        private IActorRef CreateDiscoveryService(ExtendedActorSystem system, Type type, Config config, string dispatcher, string name)
        {
            try
            {
                return system.SystemActorOf(Props.Create(type, config).WithDispatcher(dispatcher), name);
            }
            catch (Exception)
            {
                return system.SystemActorOf(Props.Create(type).WithDispatcher(dispatcher), name);
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