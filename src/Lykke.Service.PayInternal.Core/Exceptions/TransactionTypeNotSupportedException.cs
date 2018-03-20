using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransactionTypeNotSupportedException : Exception
    {
        public TransactionTypeNotSupportedException() : base("Transaction type not supported")
        {
        }

        public TransactionTypeNotSupportedException(string message) : base(message)
        {
        }

        public TransactionTypeNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionTypeNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
