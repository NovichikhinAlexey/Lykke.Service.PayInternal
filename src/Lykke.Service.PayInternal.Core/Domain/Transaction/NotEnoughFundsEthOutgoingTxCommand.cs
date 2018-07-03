namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public class NotEnoughFundsEthOutgoingTxCommand : IBlockchainTypeHolder
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
