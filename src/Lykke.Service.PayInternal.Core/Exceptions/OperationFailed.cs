using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OperationFailed : Exception
    {
        public OperationFailed()
        {
        }

        public OperationFailed(string message) : base(message)
        {
        }

        public OperationFailed(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationFailed(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
