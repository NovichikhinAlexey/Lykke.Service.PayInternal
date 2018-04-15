using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class DuplicateMerchantApiKeyException : Exception
    {
        public DuplicateMerchantApiKeyException()
        {
        }

        public DuplicateMerchantApiKeyException(string apiKey) : base(
            "Merchant with the same api key already exists")
        {
            ApiKey = apiKey;
        }

        public DuplicateMerchantApiKeyException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected DuplicateMerchantApiKeyException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        public string ApiKey { get; set; }
    }
}
