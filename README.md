## Akka.Cluster.Discovery

A set of libraries, that allows to manage a set of Akka.NET cluster seed nodes using a provided 3rd party service.

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
				listener-url = "http://127.0.0.1:8500"
			}
		}
	}
");

using (var system = ActorSystem.Create())
{
	// this line triggers discovery service initialization
	// and will join or initialize current actor system to the cluster
	ClusterDiscovery.Run(system);

	Console.ReadLine();
}
```

### Plans for the future

- Implement missing extensions.
- Limit configuration capabilities based on roles (so that only subset of actor system may be used as seed nodes).
- Implement `DiscoveryService`-based auto downing provider.
- Take into account actor system restarts (change of the corresponding unique address id).