## Akka.Cluster.Discovery

Common libraries, that allows to manage a set of Akka.NET cluster seed nodes using a provided 3rd party service.

Current status:

- [ ] Lockfile (dev only)
- [x] Akka.Cluster.Discovery.Consul
- [ ] Akka.Cluster.Discovery.Etcd
- [ ] Akka.Cluster.Discovery.ServiceFabric
- [ ] Akka.Cluster.Discovery.Zookeeper

### Example

This example uses [Consul](https://www.consul.io/) for cluster seed node discovery.

```csharp
using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Cluster.Discovery;

var config = ConfigurationFactory.Parse(@"
  akka {
    actor.provider = cluster
    cluster.discovery {
      provider = akka.cluster.discovery.consul
      consul {
        listener-url = ""http://127.0.0.1:8500""
        class = ""Akka.Cluster.Discovery.Consul.ConsulDiscoveryService, Akka.Cluster.Discovery.Consul""
      }
    }
}");

using (var system = ActorSystem.Create())
{
	// this line triggers discovery service initialization
	// and will join or initialize current actor system to the cluster
	await ClusterDiscovery.JoinAsync(system);

	Console.ReadLine();
}
```

## Configuration

```hocon
# Cluster discovery namespace
akka.cluster.discovery {
	
	# Path to a provider configuration used in for cluster discovery. Example:
	# 1. akka.cluster.disovery.consul
	provider = "akka.cluster.disovery.consul"

	# A configuration used by consult-based discovery service
	consul {
		
		# A fully qualified type name with assembly name of a discovery service class 
		# used by the cluster discovery plugin.
		class = "Akka.Cluster.Discovery.Consul.ConsulDiscoveryService, Akka.Cluster.Discovery.Consul"

		# Define a dispatcher type used by discovery service actor.
		dispatcher = "akka.actor.default-dispatcher"

		# Time interval in which a `alive` signal will be send by a discovery service
		# to fit the external service TTL (time to live) expectations. 
		alive-interval = 5s

		# Time to live given for a discovery service to be correctly acknowledged as
		# alive by external monitoring service. It must be higher than `alive-interval`. 
		alive-timeout = 1m

		# Interval in which current cluster node will reach for a discovery service
		# to retrieve data about registered node updates. Nodes, that have been detected
		# as "lost" from service discovery provider, will be downed and removed from the cluster. 
		refresh-interval = 1m

		# Maximum number of retries given for a discovery service to register itself
		# inside 3rd party provider before hitting hard failure. 
		join-retries = 3

		# In case if lock-based discovery service won't be able to acquire the lock,
		# it will retry to do it again after some time, max up to the number of times 
		# described by `join-retries` setting value.
		lock-retry-interval = 250ms

		# An URL address on with Consul listener service can be found.
		listener-url = "http://127.0.0.1:8500"

		# A Consul datacenter.
		datacenter = ""

		# A Consul token.
		token = ""

		# Timeout for a Consul client connection requests.
		wait-time = 5s

		# A timeout configured for consul to mark a time to live given for a node
		# before it will be marked as unhealthy. Must be greater than `alive-interval` and less than `alive-timeout`.
		service-check-ttl = 15s
	}
}
```

### Plans for the future

- Implement missing extensions.
- Limit configuration capabilities based on roles (so that only subset of actor system may be used as seed nodes).
- Take into account actor system restarts (change of the corresponding unique address id).
