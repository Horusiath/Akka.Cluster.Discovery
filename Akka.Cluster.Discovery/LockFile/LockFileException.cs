using System;
using Akka.Actor;

namespace Akka.Cluster.Discovery.LockFile
{
    public class LockFileException : AkkaException
    {
        public LockFileException(string message, Exception cause = null) : base(message, cause)
        {
        }
    }
}