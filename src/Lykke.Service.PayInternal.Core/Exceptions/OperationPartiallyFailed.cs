using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OperationPartiallyFailed : Exception
    {
        public OperationPartiallyFailed()
        {
        }

        public OperationPartiallyFailed(string message) : base(message)
        {
        }

        public OperationPartiallyFailed(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationPartiallyFailed(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
