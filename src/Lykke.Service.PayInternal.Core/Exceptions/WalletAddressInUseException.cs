using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAddressInUseException : Exception
    {
        public WalletAddressInUseException()
        {
        }

        public WalletAddressInUseException(string message) : base(message)
        {
        }

        public WalletAddressInUseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAddressInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WalletAddressInUseException(string walletAddress, BlockchainType blockchain) : base(
            "Wallet address is already in use")
        {
            WalletAddress = walletAddress;

            Blockchain = blockchain;
        }

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
