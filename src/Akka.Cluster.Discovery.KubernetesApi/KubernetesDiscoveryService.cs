using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using k8s;
using k8s.Models;

namespace Akka.Cluster.Discovery.KubernetesApi
{
    public class KubernetesDiscoveryService : LocklessDiscoveryService
    {
        #region Internal classes

        /// <summary>
        /// Message scheduled by <see cref="KubernetesDiscoveryService"/> for itself. 
        /// Used to trigger periodic restart of k8s client.
        /// </summary>
        public sealed class RestartClient
        {
            public static RestartClient Instance { get; } = new RestartClient();

            private RestartClient()
            {
            }
        }

        #endregion

        private readonly KubernetesSettings settings;
        private IKubernetes k8s;
        private readonly ILoggingAdapter logger = Context.GetLogger();
        
        private readonly string actorSystemName;
        private readonly string labelSelector;

        public KubernetesDiscoveryService(Config config) 
            : this(new KubernetesSettings(config))
        {            
        }

        public KubernetesDiscoveryService(KubernetesSettings settings)
            : this(CreateKubernetesClient(settings), settings)
        {
        }

        public KubernetesDiscoveryService(IKubernetes kubernetesClient, KubernetesSettings settings) : base(settings)
        {
            this.actorSystemName = ((ExtendedActorSystem)Context.System).Provider.DefaultAddress.System;
            this.k8s = kubernetesClient;
            this.settings = settings;

            labelSelector = $"akka-cluster={actorSystemName}";
            if (!string.IsNullOrWhiteSpace(settings.LabelSelector))
                labelSelector = settings.LabelSelector;
        }

        private static IKubernetes CreateKubernetesClient(KubernetesSettings settings)
        {
            var k8sClientConfig = KubernetesClientConfiguration.InClusterConfig();
            k8sClientConfig.Namespace = settings.KubernetesNamespace;
            return new Kubernetes(k8sClientConfig);
        }

        private Address DeterminePodAddress(V1Pod pod)
        {
            if (!int.TryParse(pod.GetAnnotation("akka.remote.dot-netty.tcp.port"), out var port))
                port = Cluster.SelfAddress.Port ?? 0;
            var host = pod.Status.PodIP;
            return new Address("akka.tcp", actorSystemName, host, port);
        }

        protected override async Task<IEnumerable<Address>> GetNodesAsync(bool onlyAlive)
        {
            logger.Debug("Refreshing nodes from k8s...");
            try
            {
                var pods = await k8s.ListNamespacedPodAsync(settings.KubernetesNamespace, labelSelector: settings.LabelSelector);

#if (DEBUG)
                logger.Debug($"{pods.Items.Count} matching pods found: {string.Join(", ", pods.Items.Select(pod => $"{pod.Status.PodIP}:{pod.Status.Phase} ({string.Join("; ", pod.Status.Conditions.Select(c => $"{c.Type}={c.Status} (Msg: {c.Message}, Reason: {c.Reason})"))})"))}");
#endif

                var nodes = pods.Items
                    .Where(IsReady)
                    .Select(DeterminePodAddress)
                    .ToList();

                if (!nodes.Contains(Cluster.SelfAddress))
                    nodes.Add(Cluster.SelfAddress);

#if (DEBUG)
                logger.Debug($"{nodes.Count} nodes discovered: {string.Join(", ", nodes.Select(address => address.ToString()))}");
#endif

                return nodes;
            }
            catch (Exception error)
            {
                logger.Error(error, "Could not poll k8s API for nodes: {0}", error);
                throw;
            }

            bool IsReady(V1Pod pod)
            {
                if (pod?.Status?.PodIP == null)
                    return false;

                if (!onlyAlive)
                    return true;

                return pod.Status.Phase.Equals("Running") && pod.Status.Conditions.All(x => x.Status.Equals("True"));
            }
        }

        protected override Task RegisterNodeAsync(MemberEntry node) => Task.CompletedTask;
        protected override Task DeregisterNodeAsync(MemberEntry node) => Task.CompletedTask;
        protected override Task MarkAsAliveAsync(MemberEntry node) => Task.CompletedTask;
    }
}
