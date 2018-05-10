using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnexpectedAssetPairPriceMethodException : Exception
    {
        public UnexpectedAssetPairPriceMethodException()
        {
        }

        public UnexpectedAssetPairPriceMethodException(PriceMethod priceMethod) : base("Unexpected price method")
        {
            PriceMethod = priceMethod;
        }

        public UnexpectedAssetPairPriceMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedAssetPairPriceMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PriceMethod PriceMethod { get; set; }
    }
}
