using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class BilBlockchainTypeNotSupported : Exception
    {
        public BilBlockchainTypeNotSupported()
        {
        }

        public BilBlockchainTypeNotSupported(string blockchain) : base("BIL Blockchain type not supported")
        {
            Blockchain = blockchain;
        }

        public BilBlockchainTypeNotSupported(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BilBlockchainTypeNotSupported(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Blockchain { get; set; }
    }
}
