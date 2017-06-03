#region copyright
// -----------------------------------------------------------------------
// <copyright file="EtcdSettings.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Configuration;

namespace Akka.Cluster.Discovery.Etcd
{
    public class EtcdSettings : ClusterDiscoverySettings
    {
        public EtcdSettings(Config config) : base(config)
        {
        }
    }
}