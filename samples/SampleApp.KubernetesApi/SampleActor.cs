using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

namespace SampleApp.KubernetesApi
{
    public class SampleActor : ReceiveActor
    {
        private readonly Cluster cluster;
        private readonly ILoggingAdapter logger = Context.GetLogger();

        public SampleActor()
        {
            cluster = Cluster.Get(Context.System);

            Receive<ClusterEvent.IMemberEvent>(e =>
            {
                logger.Debug($"Member event: {e}");
            });
            Receive<ClusterEvent.IReachabilityEvent>(e =>
            {
                logger.Debug($"Reachability event: {e}");
            });

            cluster.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.IReachabilityEvent));
        }

        protected override void PostStop()
        {
            cluster.Unsubscribe(Self);
            base.PostStop();
        }
    }
}