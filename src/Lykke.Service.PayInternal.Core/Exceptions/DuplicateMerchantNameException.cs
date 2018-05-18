using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class DuplicateMerchantNameException : Exception
    {
        public DuplicateMerchantNameException()
        {
        }

        public DuplicateMerchantNameException(string merchantName) : base("Merchant with the same name already exists")
        {
            MerchantName = merchantName;
        }

        public DuplicateMerchantNameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateMerchantNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantName { get; set; }
    }
}
