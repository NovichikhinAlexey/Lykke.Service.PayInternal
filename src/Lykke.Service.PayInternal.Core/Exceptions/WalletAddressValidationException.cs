using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAddressValidationException : Exception
    {
        public WalletAddressValidationException()
        {
        }

        public WalletAddressValidationException(BlockchainType blockchain, string address) : base("Wallet address is invalid")
        {
            Blockchain = blockchain;
            Address = address;
        }

        public WalletAddressValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAddressValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }

        public string Address { get; set; }
    }
}
