using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAddressBalanceException : Exception
    {
        public WalletAddressBalanceException()
        {
        }

        public WalletAddressBalanceException(BlockchainType blockchain, string address) : base("Wallet address balance exception")
        {
            Blockchain = blockchain;
            Address = address;
        }

        public WalletAddressBalanceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAddressBalanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }

        public string Address { get; set; }
    }
}
