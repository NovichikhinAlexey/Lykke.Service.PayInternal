using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAddressAllocationException : Exception
    {
        public WalletAddressAllocationException()
        {
        }

        public WalletAddressAllocationException(BlockchainType blockchain) : base("Wallet address allocation failed")
        {
            Blockchain = blockchain;
        }

        public WalletAddressAllocationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAddressAllocationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }
    }
}
