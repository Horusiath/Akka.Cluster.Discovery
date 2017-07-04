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
    public class ConsulDiscoveryService : LockingDiscoveryService
    {
        private readonly IConsulClient consul;
        private readonly ConsulSettings settings;
        private IDistributedLock distributedLock;

        public ConsulDiscoveryService(Config config) : this(new ConsulSettings(config)) 
        {
            
        }

        public ConsulDiscoveryService(ConsulSettings settings) 
            : this(new ConsulClient(config => ConfigureClient(config, settings)), settings)
        {
        }

        public ConsulDiscoveryService(IConsulClient consulClient, ConsulSettings settings) : base(settings)
        {
            consul = consulClient;
            this.settings = settings;
        }

        protected override async Task<bool> LockAsync(string key)
        {
            distributedLock = await consul.AcquireLock(key);
            return distributedLock.IsHeld;
        }

        protected override async Task UnlockAsync(string key)
        {
            await distributedLock.Release();
        }

        protected override async Task<IEnumerable<Address>> GetAliveNodesAsync()
        {
            var services = await consul.Health.Service(Context.System.Name);

            var result =
                from x in services.Response
                where Equals(x.Checks[1].Status, HealthStatus.Passing)
                select Address.Parse(x.Service.ID);

            return result;
        }

        protected override async Task RegisterNodeAsync(MemberEntry entry)
        {
            if (!entry.Address.Port.HasValue) throw new ArgumentException($"Cluster address {entry.Address} doesn't have a port specified");

            var addr = entry.Address;
            var id = $"{addr.System}@{addr.Host}:{addr.Port}";
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
                    DeregisterCriticalServiceAfter = settings.AliveTimeout,
                }
            };
            await consul.Agent.ServiceDeregister(registration.ID);
            await consul.Agent.ServiceRegister(registration);
        }

        protected override async Task MarkAsAliveAsync(MemberEntry entry)
        {
            var addr = entry.Address;
            await consul.Agent.PassTTL($"service:{addr.System}@{addr.Host}:{addr.Port}", string.Empty);
        }

        protected override void PostStop()
        {
            base.PostStop();
            consul.Dispose();
        }

        private static void ConfigureClient(ConsulClientConfiguration clientConfig, ConsulSettings settings)
        {
            clientConfig.Address = settings.ListenerUrl;
            clientConfig.Datacenter = settings.Datacenter;
            clientConfig.Token = settings.Token;
            clientConfig.WaitTime = settings.WaitTime;
        }
    }
}