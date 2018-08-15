using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class PaymentOperationPartiallyFailedException : PaymentOperationFailedException
    {
        public PaymentOperationPartiallyFailedException()
        {
        }

        public PaymentOperationPartiallyFailedException(IEnumerable<string> transferErrors) : base(transferErrors)
        {
        }

        public PaymentOperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PaymentOperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
