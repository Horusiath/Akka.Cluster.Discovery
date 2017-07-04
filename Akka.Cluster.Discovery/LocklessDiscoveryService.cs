#region copyright
// -----------------------------------------------------------------------
// <copyright file="LocklessDiscoveryService.cs" company="Akka.NET Project">
//    Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//    Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util;

namespace Akka.Cluster.Discovery
{
    /// <summary>
    /// Unlike <see cref="LockingDiscoveryService"/>, which tries to take an advantage of
    /// distributed locks provided by 3rd party service, this version of <see cref="DiscoveryService"/>
    /// work in turn based system: every time a node tries to register itself, it will pick a random
    /// number (upper bound set in <see cref="LocklessClusterDiscoverySettings.MaxTurns"/>) and multiply
    /// it by <see cref="LocklessClusterDiscoverySettings.TurnPeriod"/>. This is the amount of time it
    /// will skip itself before trying to register itself and/or establish the cluster.
    /// </summary>
    public abstract class LocklessDiscoveryService : DiscoveryService
    {
        private readonly LocklessClusterDiscoverySettings settings;
        protected LocklessDiscoveryService(LocklessClusterDiscoverySettings settings) : base(settings)
        {
            this.settings = settings;
        }

        protected override void SendJoinSignal()
        {
            var waitTurns = ThreadLocalRandom.Current.Next(settings.MaxTurns);
            var waitFor = new TimeSpan(settings.TurnPeriod.Ticks * waitTurns);
            Context.System.Scheduler.ScheduleTellOnce(waitFor, Self, Join.Instance, ActorRefs.NoSender);
        }
    }

    public class LocklessClusterDiscoverySettings : ClusterDiscoverySettings
    {
        /// <summary>
        /// Time period given for the service to perform all of the necessary logic,
        /// this includes retrieving currently registered nodes and trying to register itself.
        /// </summary>
        public TimeSpan TurnPeriod { get; }
        public int MaxTurns { get; }

        public LocklessClusterDiscoverySettings() : base()
        {
            TurnPeriod = TimeSpan.FromMilliseconds(500);
            MaxTurns = 10;
        }

        public LocklessClusterDiscoverySettings(TimeSpan aliveInterval, TimeSpan aliveTimeout, int joinRetries, TimeSpan turnPeriod, int maxTurns) 
            : base(aliveInterval, aliveTimeout, joinRetries)
        {
            TurnPeriod = turnPeriod;
            MaxTurns = maxTurns;
        }

        public LocklessClusterDiscoverySettings(Config config) 
            : base(config)
        {
            TurnPeriod = config.GetTimeSpan("turn-period", TimeSpan.FromMilliseconds(500));
            MaxTurns = config.GetInt("max-turns", 10);
        }
    }
}