using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class ExchangeOperationFailedException : Exception
    {
        public ExchangeOperationFailedException()
        {
        }

        public ExchangeOperationFailedException(IEnumerable<string> transferErrors) : base("Exchange operation failed")
        {
            TransferErrors = transferErrors;
        }

        public ExchangeOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExchangeOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> TransferErrors { get; set; }
    }
}
