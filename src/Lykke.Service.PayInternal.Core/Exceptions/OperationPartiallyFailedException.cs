using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OperationPartiallyFailedException : Exception
    {
        public OperationPartiallyFailedException()
        {
        }

        public OperationPartiallyFailedException(string message) : base(message)
        {
        }

        public OperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
