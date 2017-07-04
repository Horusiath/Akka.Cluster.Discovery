#region copyright
// -----------------------------------------------------------------------
// <copyright file="MemberEntry.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Akka.Actor;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// Member entry contain most basic data about current cluster node, 
    /// that can be used to join or initialize the cluster.
    /// </summary>
    public class MemberEntry
    {
        /// <summary>
        /// Name of the actor system being part of the cluster. All cluster
        /// actor systems must share the same name.
        /// </summary>
        public string ClusterName { get; }

        /// <summary>
        /// Address on which current actor system is listening on.
        /// </summary>
        public Address Address { get; }

        /// <summary>
        /// List of roles attached to a current actor system.
        /// </summary>
        public IEnumerable<string> Roles { get; }

        public MemberEntry(string clusterName, Address address, IEnumerable<string> roles)
        {
            ClusterName = clusterName;
            Address = address;
            Roles = roles;
        }
    }
}