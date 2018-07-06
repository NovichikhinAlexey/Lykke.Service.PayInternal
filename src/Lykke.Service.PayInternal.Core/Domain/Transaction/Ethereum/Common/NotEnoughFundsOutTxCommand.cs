namespace Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common
{
    /// <summary>
    /// Command to fail outgoing transaction because of funds insufficiency
    /// </summary>
    public class NotEnoughFundsOutTxCommand : IBlockchainTypeHolder
    {
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public string OperationId { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public BlockchainType Blockchain { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}
