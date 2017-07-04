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

        protected override Task<IEnumerable<Address>> GetAliveNodesAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task RegisterNodeAsync(MemberEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override Task MarkAsAliveAsync(MemberEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void SendJoinSignal()
        {
            throw new NotImplementedException();
        }
    }
}