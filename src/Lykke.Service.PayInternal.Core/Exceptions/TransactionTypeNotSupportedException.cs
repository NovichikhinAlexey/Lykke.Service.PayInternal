using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransactionTypeNotSupportedException : Exception
    {
        public TransactionTypeNotSupportedException()
        {
        }

        public TransactionTypeNotSupportedException(string type) : base("Transaction type not supported")
        {
            Type = type;
        }

        public TransactionTypeNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionTypeNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Type { get; set; }
    }
}
