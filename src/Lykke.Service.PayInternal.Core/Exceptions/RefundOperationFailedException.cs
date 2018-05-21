using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class RefundOperationFailedException : Exception
    {
        public RefundOperationFailedException()
        {
        }

        public RefundOperationFailedException(IEnumerable<string> transferErrors) : base("Refund operation failed")
        {
            TransferErrors = transferErrors;
        }

        public RefundOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RefundOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> TransferErrors { get; set; }
    }
}
