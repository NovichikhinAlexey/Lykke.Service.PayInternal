using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class DistributedLockAcquireException : Exception
    {
        public DistributedLockAcquireException()
        {
        }

        public DistributedLockAcquireException(string key) : base("Couldn't acquire lock")
        {
            Key = key;
        }

        public DistributedLockAcquireException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DistributedLockAcquireException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Key { get; set; }
    }
}
