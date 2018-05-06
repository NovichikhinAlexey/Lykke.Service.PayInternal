using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MarkupNotFoundException : Exception
    {
        public MarkupNotFoundException()
        {
        }

        public MarkupNotFoundException(string merchantId, string assetPairId) : base("Markup not found")
        {
            MerchantId = merchantId;
            AssetPairId = assetPairId;
        }

        public MarkupNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MarkupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; set; }

        public string AssetPairId { get; set; }
    }
}
