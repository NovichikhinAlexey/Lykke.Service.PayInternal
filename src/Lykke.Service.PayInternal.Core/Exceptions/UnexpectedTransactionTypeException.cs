using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnexpectedTransactionTypeException : Exception
    {
        public UnexpectedTransactionTypeException()
        {
        }

        public UnexpectedTransactionTypeException(TransactionType transactionType) : base("Unexpected transaction type")
        {
            TransactionType = transactionType;
        }

        public UnexpectedTransactionTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedTransactionTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionType TransactionType { get; set; }
    }
}
