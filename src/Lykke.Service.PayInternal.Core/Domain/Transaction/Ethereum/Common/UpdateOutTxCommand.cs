using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common
{
    /// <summary>
    /// Command to update any outgoing transaction
    /// </summary>
    public class UpdateOutTxCommand : IBlockchainTypeHolder
    {
        public string Hash { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string OperationId { get; set; }
        public string BlockId { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public BlockchainType Blockchain { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public DateTime? FirstSeen { get; set; }
    }
}
