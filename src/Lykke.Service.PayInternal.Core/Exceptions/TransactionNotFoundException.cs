using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public TransactionNotFoundException()
        {
        }

        public TransactionNotFoundException(string transactionId) : base("Transaction not found")
        {
            TransactionId = transactionId;
        }

        public TransactionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string TransactionId { get; set; }
    }
}
