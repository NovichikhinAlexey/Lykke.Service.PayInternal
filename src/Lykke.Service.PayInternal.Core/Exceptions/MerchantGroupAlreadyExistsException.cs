using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MerchantGroupAlreadyExistsException : Exception
    {
        public MerchantGroupAlreadyExistsException()
        {
        }

        public MerchantGroupAlreadyExistsException(string group) : base("Merchant group already exists")
        {
            Group = group;
        }

        public MerchantGroupAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantGroupAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Group { get; set; }
    }
}
