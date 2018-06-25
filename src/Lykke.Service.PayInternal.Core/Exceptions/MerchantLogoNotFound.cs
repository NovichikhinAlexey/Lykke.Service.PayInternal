using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    [Serializable]
    public class MerchantLogoNotFoundException : Exception
    {
        public MerchantLogoNotFoundException()
        {
        }

        public MerchantLogoNotFoundException(string merchantId) : base("Merchant logo not found")
        {
            MerchantId = merchantId;
        }

        public MerchantLogoNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantLogoNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; }
    }
}
