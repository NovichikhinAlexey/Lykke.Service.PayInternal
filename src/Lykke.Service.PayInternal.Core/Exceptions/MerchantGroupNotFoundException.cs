using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MerchantGroupNotFoundException : Exception
    {
        public MerchantGroupNotFoundException()
        {
        }

        public MerchantGroupNotFoundException(string merchantGroupId) : base("Merchant group not found")
        {
            MerchantGroupId = merchantGroupId;
        }

        public MerchantGroupNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantGroupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantGroupId { get; set; }
    }
}
