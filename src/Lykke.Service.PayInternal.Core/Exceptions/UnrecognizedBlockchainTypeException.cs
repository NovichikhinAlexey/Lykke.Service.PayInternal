using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnrecognizedBlockchainTypeException : Exception
    {
        public UnrecognizedBlockchainTypeException()
        {
        }

        public UnrecognizedBlockchainTypeException(BlockchainType blockchain) : base("Unrecognized blockchain type")
        {
            Blockchain = blockchain;
        }

        public UnrecognizedBlockchainTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnrecognizedBlockchainTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }
    }
}
