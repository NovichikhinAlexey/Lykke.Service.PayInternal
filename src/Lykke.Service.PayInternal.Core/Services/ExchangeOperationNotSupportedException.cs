using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Services
{
    public class ExchangeOperationNotSupportedException : Exception
    {
        public ExchangeOperationNotSupportedException()
        {
        }

        public ExchangeOperationNotSupportedException(string message) : base(message)
        {
        }

        public ExchangeOperationNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExchangeOperationNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
