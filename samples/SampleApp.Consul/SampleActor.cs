using System;
using Akka.Actor;
using Akka.Cluster;

namespace SampleApp
{
    public class SampleActor : ReceiveActor
    {
        private readonly Cluster cluster;

        public SampleActor()
        {
            cluster = Cluster.Get(Context.System);

            Receive<ClusterEvent.IMemberEvent>(e =>
            {
                Console.WriteLine($"Member event: {e}");
            });
            Receive<ClusterEvent.IReachabilityEvent>(e =>
            {
                Console.WriteLine($"Reachability event: {e}");
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