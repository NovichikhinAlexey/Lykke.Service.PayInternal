using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class BlockchainTypeNotSupported : Exception
    {
        public BlockchainTypeNotSupported()
        {
        }

        public BlockchainTypeNotSupported(BlockchainType blockchain) : base("Blockchain type not supported")
        {
            Blockchain = blockchain;
        }

        public BlockchainTypeNotSupported(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BlockchainTypeNotSupported(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }
    }
}
