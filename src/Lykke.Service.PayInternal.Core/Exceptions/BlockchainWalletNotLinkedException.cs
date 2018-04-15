using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class BlockchainWalletNotLinkedException : Exception
    {
        public BlockchainWalletNotLinkedException()
        {
        }

        public BlockchainWalletNotLinkedException(string message) : base(message)
        {
        }

        public BlockchainWalletNotLinkedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BlockchainWalletNotLinkedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainWalletNotLinkedException(string walletAddress, BlockchainType blockchain) : base ("Blockchain wallet not associated with virtual wallet")
        {
            WalletAddress = walletAddress;

            Blockchain = blockchain;
        }

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
