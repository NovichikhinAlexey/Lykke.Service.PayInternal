using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public class CompleteEthOutgoingTxCommand : IBlockchainTypeHolder
    {
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public string OperationId { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public BlockchainType Blockchain { get; set; }
        public decimal Amount { get; set; }
        public string BlockId { get; set; }
        public DateTime? FirstSeen { get; set; }
        public string Hash { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}
