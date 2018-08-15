using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException()
        {
        }

        public InsufficientFundsException(string walletAdress, string assetId) : base("Insufficient funds")
        {
            WalletAddress = walletAdress;
            AssetId = assetId;
        }

        public InsufficientFundsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InsufficientFundsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string WalletAddress { get; set; }

        public string AssetId { get; set; }
    }
}
