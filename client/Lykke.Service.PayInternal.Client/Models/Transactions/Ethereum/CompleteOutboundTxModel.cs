namespace Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum
{
    /// <summary>
    /// Complete ountbound transaction request details
    /// </summary>
    public class CompleteOutboundTxModel
    {
        /// <summary>
        /// Gets or sets identity type
        /// </summary>
        public TransactionIdentityType IdentityType { get; set; }

        /// <summary>
        /// Gets or sets identity
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Gets or sets operatoin id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or sets workflow type
        /// </summary>
        public WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets blockchain type
        /// </summary>
        public BlockchainType Blockchain { get; set; }
    }
}
