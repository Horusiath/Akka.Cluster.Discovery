#region copyright
// -----------------------------------------------------------------------
// <copyright file="ConsulDiscoveryService.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Consul;

namespace Akka.Cluster.Discovery.Consul
{
    public class ConsulDiscoveryService : DiscoveryService
    {
        private readonly IConsulClient consul;
        private readonly ConsulSettings settings;
        private IDistributedLock distributedLock;

        public ConsulDiscoveryService(Config config) : this(new ConsulSettings(config)) 
        {
            
        }

        public ConsulDiscoveryService(ConsulSettings settings) 
            : this(new ConsulClient(/*TODO: configure*/), settings)
        {
        }

        public ConsulDiscoveryService(IConsulClient consulClient, ConsulSettings settings) : base(settings)
        {
            consul = consulClient;
            this.settings = settings;
        }

        protected override async Task<bool> LockAsync(string systemName)
        {
            distributedLock = await consul.AcquireLock(systemName);
            return distributedLock.IsHeld;
        }

        protected override async Task UnlockAsync(string systemName)
        {
            await distributedLock.Release();
        }

        protected override async Task<IEnumerable<Address>> GetAliveNodesAsync(string systemName)
        {
            var services = await consul.Health.Service(systemName);

            var result =
                from x in services.Response
                where Equals(x.Checks[1].Status, HealthStatus.Passing)
                select Address.Parse(x.Service.ID);

            return result;
        }

        protected override async Task RegisterNodeAsync(MemberEntry entry)
        {
            if (!entry.Address.Port.HasValue) throw new ArgumentException($"Cluster address {entry.Address} doesn't have a port specified");

            var id = entry.Address.ToString();
            var registration = new AgentServiceRegistration
            {
                ID = id,
                Name = entry.ClusterName,
                Tags = entry.Roles.ToArray(),
                Address = entry.Address.Host,
                Port = entry.Address.Port.Value,
                Check = new AgentServiceCheck
                {
                    TTL = settings.AliveInterval,
                    // deregister after 3 activity turns failed
                    DeregisterCriticalServiceAfter = new TimeSpan(settings.AliveInterval.Ticks * 3),
                }
            };
            await consul.Agent.ServiceDeregister(registration.ID);
            await consul.Agent.ServiceRegister(registration);
        }

        protected override async Task MarkAsAliveAsync(MemberEntry entry)
        {
            await consul.Agent.PassTTL(entry.Address.ToString(), string.Empty);
        }

        protected override void PostStop()
        {
            base.PostStop();
            consul.Dispose();
        }
    }
}