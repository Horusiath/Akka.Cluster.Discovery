#region copyright
// -----------------------------------------------------------------------
// <copyright file="ConsulSettings.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Configuration;

namespace Akka.Cluster.Discovery.Consul
{
    public class ConsulSettings : ClusterDiscoverySettings
    {
        public ConsulSettings(Config config) : base(config)
        {
            
        }
    }
}