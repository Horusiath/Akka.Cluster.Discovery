#region copyright
// -----------------------------------------------------------------------
// <copyright file="ZookeeperDiscoveryService.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using org.apache.zookeeper;

namespace Akka.Cluster.Discovery.Zookeeper
{
    public class ZookeeperDiscoveryService : DiscoveryService
    {
        private readonly ZooKeeper zookeeper;
        private Transaction transaction;

        public ZookeeperDiscoveryService(ZooKeeper zookeeper, ClusterDiscoverySettings settings) : base(settings)
        {
            this.zookeeper = zookeeper;
        }

        protected override async Task<bool> LockAsync(string systemName)
        {
            transaction = zookeeper.transaction();
            return true;
        }

        protected override async Task UnlockAsync(string systemName)
        {
            await transaction.commitAsync();
            transaction = null;
        }

        protected override async Task<IEnumerable<Address>> GetAliveNodesAsync(string systemName)
        {
            throw new NotImplementedException();
        }

        protected override async Task RegisterNodeAsync(MemberEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override async Task MarkAsAliveAsync(MemberEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void PostStop()
        {
            base.PostStop();
            
        }
    }
}