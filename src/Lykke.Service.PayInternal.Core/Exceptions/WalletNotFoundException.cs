using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletNotFoundException : Exception
    {
        public WalletNotFoundException()
        {
        }

        public WalletNotFoundException(string walletAddress) : base("Wallet address not found")
        {
            WalletAddress = walletAddress;
        }

        public WalletNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string WalletAddress { get; set; }
    }
}
