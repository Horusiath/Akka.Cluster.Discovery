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
    public class MemberEntry
    {
        public string ClusterName { get; }
        public Address Address { get; }
        public IEnumerable<string> Roles { get; }

        public MemberEntry(string clusterName, Address address, IEnumerable<string> roles)
        {
            ClusterName = clusterName;
            Address = address;
            Roles = roles;
        }
    }
}