using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MerchantWalletNotFoundException : Exception
    {
        public MerchantWalletNotFoundException()
        {
        }

        public MerchantWalletNotFoundException(string merchantId, BlockchainType network, string walletAddress) : base("Merchant wallet not found")
        {
            MerchantId = merchantId;
            Network = network;
            WalletAddress = walletAddress;
        }

        public MerchantWalletNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantWalletNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }
        public BlockchainType Network { get; set; }
        public string WalletAddress { get; set; }
    }
}
