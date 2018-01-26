using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when requested merchant cannot be found.
    /// </summary>
    [Serializable]
    public class MerchantNotFoundException : Exception
    {
        public MerchantNotFoundException()
        {
        }

        public MerchantNotFoundException(string merchantId)
            : base("Merchant not found.")
        {
            MerchantId = merchantId;
        }

        public MerchantNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantId { get; }
    }
}
