using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class PaymentOperationFailedException : Exception
    {
        public PaymentOperationFailedException()
        {
        }

        public PaymentOperationFailedException(IEnumerable<string> transferErrors) : base("Payment operation failed")
        {
            TransferErrors = transferErrors;
        }

        public PaymentOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PaymentOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> TransferErrors { get; set; }
    }
}
