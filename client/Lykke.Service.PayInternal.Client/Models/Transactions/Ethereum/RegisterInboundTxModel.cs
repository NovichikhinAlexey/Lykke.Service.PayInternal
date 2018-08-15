using System;

namespace Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum
{
    /// <summary>
    /// Ethereum inbound transaction registration request details
    /// </summary>
    public class RegisterInboundTxModel
    {
        /// <summary>
        /// Gets or sets transaction hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets source address
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets destination address
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets operatoin id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or sets block id
        /// </summary>
        public string BlockId { get; set; }

        /// <summary>
        /// Gets or sets workflow type
        /// </summary>
        public WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets blockchain type
        /// </summary>
        public BlockchainType Blockchain { get; set; }

        /// <summary>
        /// Gets or sets transactoin identity type
        /// </summary>
        public TransactionIdentityType IdentityType { get; set; }

        /// <summary>
        /// Gets or sets identity
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Gets or sets firstsenn date
        /// </summary>
        public DateTime? FirstSeen { get; set; }
    }
}
