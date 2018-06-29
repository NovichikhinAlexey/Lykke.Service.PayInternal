using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MerchantWalletOwnershipException : Exception
    {
        public MerchantWalletOwnershipException()
        {
        }

        public MerchantWalletOwnershipException(string merchantId, string walletAdress) : base("Merchant is not the owner of the wallet")
        {
            MerchantId = merchantId;
            WalletAddress = walletAdress;
        }

        public MerchantWalletOwnershipException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantWalletOwnershipException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }

        public string WalletAddress { get; set; }
    }
}
