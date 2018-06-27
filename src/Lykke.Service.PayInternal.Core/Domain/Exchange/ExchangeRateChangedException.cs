using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Domain.Exchange
{
    public class ExchangeRateChangedException : Exception
    {
        public decimal CurrentRate { get; set; }

        public ExchangeRateChangedException()
        {
        }

        public ExchangeRateChangedException(decimal currentRate):this($"Exchange rate is changed with {currentRate}.")
        {
            CurrentRate = currentRate;
        }

        public ExchangeRateChangedException(string message) : base(message)
        {
        }

        public ExchangeRateChangedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExchangeRateChangedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
