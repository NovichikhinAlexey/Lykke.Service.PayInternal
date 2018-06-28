namespace Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum
{
    /// <summary>
    /// Not enough funds ountbound transaction request details
    /// </summary>
    public class NotEnoughFundsOutboundTxModel
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
        /// Gets or sets operation id
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

        /// <summary>
        /// Gets or sets source address
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets destination address
        /// </summary>
        public string ToAddress { get; set; }
    }
}
