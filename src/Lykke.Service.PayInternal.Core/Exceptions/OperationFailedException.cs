using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OperationFailedException : Exception
    {
        public OperationFailedException()
        {
        }

        public OperationFailedException(string message) : base(message)
        {
        }

        public OperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
