#region copyright
// -----------------------------------------------------------------------
// <copyright file="MemberEntry.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// Member entry contain most basic data about current cluster node, 
    /// that can be used to join or initialize the cluster.
    /// </summary>
    public class MemberEntry : IEquatable<MemberEntry>
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

        public bool Equals(MemberEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ClusterName, other.ClusterName) && Equals(Address, other.Address) && Roles.SequenceEqual(other.Roles);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ClusterName != null ? ClusterName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Address != null ? Address.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Roles != null ? Roles.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() => $"MemberEntry(cluster: {ClusterName}, address: {Address}, roles: [{string.Join(", ", Roles)}])";
    }
}