using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class RefundOperationPartiallyFailedException : RefundOperationFailedException
    {
        public RefundOperationPartiallyFailedException()
        {
        }

        public RefundOperationPartiallyFailedException(IEnumerable<string> transferErrors) : base(transferErrors)
        {
        }

        public RefundOperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RefundOperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
