using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAddressProcessingHostException : Exception
    {
        public WalletAddressProcessingHostException()
        {
        }

        public WalletAddressProcessingHostException(string walletAddress, BlockchainType blockchain) : base("The wallet address is supposed to be processed by another instance")
        {
            WalletAddress = walletAddress;
            Blockchain = blockchain;
        }

        public WalletAddressProcessingHostException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAddressProcessingHostException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
