using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnexpectedPriceCalculationMethodException : Exception
    {
        public UnexpectedPriceCalculationMethodException()
        {
        }

        public UnexpectedPriceCalculationMethodException(PriceCalculationMethod method) : base(
            "Unexpected price calculation method")
        {
            Method = method;
        }

        public UnexpectedPriceCalculationMethodException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected UnexpectedPriceCalculationMethodException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        public PriceCalculationMethod Method { get; set; }
    }
}
