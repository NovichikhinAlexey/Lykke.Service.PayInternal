using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class DefaultMerchantWalletNotFoundException : Exception
    {
        public DefaultMerchantWalletNotFoundException()
        {
        }

        public DefaultMerchantWalletNotFoundException(string merchantId, string assetId, PaymentDirection paymentDirection) : base("Default merchant wallet not found")
        {
            MerchantId = merchantId;
            AssetId = assetId;
            PaymentDirection = paymentDirection;
        }

        public DefaultMerchantWalletNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DefaultMerchantWalletNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }
        public string AssetId { get; set; }
        public PaymentDirection PaymentDirection { get; set; }
    }
}
