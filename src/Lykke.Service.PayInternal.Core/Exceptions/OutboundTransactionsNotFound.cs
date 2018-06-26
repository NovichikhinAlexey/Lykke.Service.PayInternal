using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OutboundTransactionsNotFound : Exception
    {
        public OutboundTransactionsNotFound()
        {
        }

        public OutboundTransactionsNotFound(BlockchainType blockchain, TransactionIdentityType identityType, string identity) : base("Outbound tansactions not found")
        {
            Blockchain = blockchain;
            IdentityType = identityType;
            Identity = identity;
        }

        public OutboundTransactionsNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OutboundTransactionsNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
    }
}
