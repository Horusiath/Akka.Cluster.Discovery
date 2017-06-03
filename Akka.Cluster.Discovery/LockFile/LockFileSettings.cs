using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Discovery.LockFile
{
    public sealed class LockFileSettings : ClusterDiscoverySettings
    {
        public string Path { get; }

        public LockFileSettings()
        {
            Path = string.Empty;
        }

        public LockFileSettings(TimeSpan aliveInterval, TimeSpan lockRetryInterval, int lockRetries, string path) 
            : base(aliveInterval, lockRetryInterval, lockRetries)
        {
            Path = path;
        }

        public LockFileSettings(Config config) : base(config)
        {
            Path = config.GetString("lock-file-path");
        }
    }
}