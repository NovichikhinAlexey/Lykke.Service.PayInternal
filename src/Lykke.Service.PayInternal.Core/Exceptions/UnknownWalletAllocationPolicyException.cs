using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnknownWalletAllocationPolicyException : Exception
    {
        public UnknownWalletAllocationPolicyException()
        {
        }

        public UnknownWalletAllocationPolicyException(string policy) : base("Unknown wallet allocation policy")
        {
            Policy = policy;
        }

        public UnknownWalletAllocationPolicyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownWalletAllocationPolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Policy { get; set; }
    }
}
