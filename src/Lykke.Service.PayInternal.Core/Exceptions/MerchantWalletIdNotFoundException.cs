using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MerchantWalletIdNotFoundException : Exception
    {
        public MerchantWalletIdNotFoundException()
        {
        }

        public MerchantWalletIdNotFoundException(string merchantWalletId) : base("Merchant wallet not found")
        {
            MerchantWalletId = merchantWalletId;
        }

        public MerchantWalletIdNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MerchantWalletIdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MerchantWalletId { get; set; }
    }
}
