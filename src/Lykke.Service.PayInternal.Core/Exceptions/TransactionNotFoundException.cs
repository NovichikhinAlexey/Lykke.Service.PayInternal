using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public TransactionNotFoundException()
        {
        }

        public TransactionNotFoundException(BlockchainType blockchain, TransactionIdentityType identityType, string identity) : base("Transaction not found")
        {
            Blockchain = blockchain;
            IdentityType = identityType;
            Identity = identity;
        }

        public TransactionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }

        public TransactionIdentityType IdentityType { get; set; }

        public string Identity { get; set; }
    }
}
