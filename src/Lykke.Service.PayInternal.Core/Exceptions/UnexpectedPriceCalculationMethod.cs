using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnexpectedPriceCalculationMethod : Exception
    {
        public UnexpectedPriceCalculationMethod()
        {
        }

        public UnexpectedPriceCalculationMethod(PriceCalculationMethod method) : base(
            "Unexpected price calculation method")
        {
            Method = method;
        }

        public UnexpectedPriceCalculationMethod(string message, Exception innerException) : base(message,
            innerException)
        {
        }

        protected UnexpectedPriceCalculationMethod(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        public PriceCalculationMethod Method { get; set; }
    }
}
