using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class ZeroRateException : Exception
    {
        public ZeroRateException() : base("The calculated rate is equal to zero")
        {
        }

        public ZeroRateException(string message) : base(message)
        {
        }

        public ZeroRateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ZeroRateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
