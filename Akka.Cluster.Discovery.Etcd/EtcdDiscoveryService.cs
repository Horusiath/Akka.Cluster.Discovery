#region copyright
// -----------------------------------------------------------------------
// <copyright file="EtcdDiscoveryService.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery.Etcd
{
    public class EtcdDiscoveryService : DiscoveryService
    {
        public EtcdDiscoveryService(Config config) : this(new EtcdSettings(config))
        {
        }

        public EtcdDiscoveryService(EtcdSettings settings) : base(settings)
        {
        }

        protected override Task<IEnumerable<Address>> GetAliveNodesAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task RegisterNodeAsync(MemberEntry node)
        {
            throw new System.NotImplementedException();
        }

        protected override Task MarkAsAliveAsync(MemberEntry node)
        {
            throw new System.NotImplementedException();
        }

        protected override void SendJoinSignal()
        {
            throw new System.NotImplementedException();
        }
    }
}