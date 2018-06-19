using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class ExchangeOperationInsufficientFundsException : Exception
    {
        public ExchangeOperationInsufficientFundsException()
        {
        }

        public ExchangeOperationInsufficientFundsException(string walletAdress, string assetId) : base("Insufficient funds")
        {
            WalletAddress = walletAdress;
            AssetId = assetId;
        }

        public ExchangeOperationInsufficientFundsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExchangeOperationInsufficientFundsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string WalletAddress { get; set; }

        public string AssetId { get; set; }
    }
}
