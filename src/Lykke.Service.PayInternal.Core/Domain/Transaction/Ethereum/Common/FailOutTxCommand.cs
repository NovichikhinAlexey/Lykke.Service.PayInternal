namespace Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common
{
    /// <summary>
    /// Command to fail outgoing transaction (not defined reason)
    /// </summary>
    public class FailOutTxCommand : IBlockchainTypeHolder
    {
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public string OperationId { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public BlockchainType Blockchain { get; set; }
    }
}
