using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class LykkeOperationOrderNotFoundException : Exception
    {
        public LykkeOperationOrderNotFoundException()
        {
        }

        public LykkeOperationOrderNotFoundException(string operationId) : base("Order not found for lykke operation")
        {
            OperationId = operationId;
        }

        public LykkeOperationOrderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LykkeOperationOrderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string OperationId { get; set; }
    }
}
