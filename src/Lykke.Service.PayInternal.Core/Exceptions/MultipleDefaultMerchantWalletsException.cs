using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MultipleDefaultMerchantWalletsException : Exception
    {
        public MultipleDefaultMerchantWalletsException()
        {
        }

        public MultipleDefaultMerchantWalletsException(string merchantId, string assetId, PaymentDirection paymentDirection) : base("Found multiple default wallets for asset")
        {
            MerchantId = merchantId;
            AssetId = assetId;
            PaymentDirection = paymentDirection;
        }

        public MultipleDefaultMerchantWalletsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultipleDefaultMerchantWalletsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }
        public string AssetId { get; set; }
        public PaymentDirection PaymentDirection { get; set; }
    }
}
