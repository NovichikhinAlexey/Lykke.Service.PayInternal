using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public TransactionNotFoundException()
        {
        }

        public TransactionNotFoundException(string transactionId, BlockchainType blockchain) : base("Transaction not found")
        {
            TransactionId = transactionId;
            Blockchain = blockchain;
        }

        public TransactionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string TransactionId { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
