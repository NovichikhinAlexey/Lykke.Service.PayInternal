using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class CashoutOperationPartiallyFailedException : CashoutOperationFailedException
    {
        public CashoutOperationPartiallyFailedException()
        {
        }

        public CashoutOperationPartiallyFailedException(IEnumerable<string> transferErrors) : base(transferErrors)
        {
        }

        public CashoutOperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CashoutOperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
