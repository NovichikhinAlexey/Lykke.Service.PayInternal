using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class ExchangeOperationPartiallyFailedException : ExchangeOperationFailedException
    {
        public ExchangeOperationPartiallyFailedException()
        {
        }

        public ExchangeOperationPartiallyFailedException(IEnumerable<string> transferErrors) : base(transferErrors)
        {
        }

        public ExchangeOperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExchangeOperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
