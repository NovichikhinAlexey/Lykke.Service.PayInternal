using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MarketPriceZeroException : Exception
    {
        public MarketPriceZeroException()
        {
        }

        public MarketPriceZeroException(string priceType) : base("Market price is 0")
        {
            PriceType = priceType;
        }

        public MarketPriceZeroException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MarketPriceZeroException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string PriceType { get; set; }
    }
}
