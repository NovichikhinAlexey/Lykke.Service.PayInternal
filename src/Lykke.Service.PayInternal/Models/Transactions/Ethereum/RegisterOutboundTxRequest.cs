using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Transactions.Ethereum
{
    /// <summary>
    /// Ethereum outbound transaction registration request details
    /// </summary>
    public class RegisterOutboundTxRequest
    {
        /// <summary>
        /// Gets or sets transaction hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets source address
        /// </summary>
        [Required]
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets destination address
        /// </summary>
        [Required]
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
        [Required]
        [EnumDataType(typeof(WorkflowType), ErrorMessage = "Invalid value, possible values are: LykkePay, Airlines")]
        public WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets blockchain type
        /// </summary>
        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: Ethereum, EthereumIata")]
        public BlockchainType Blockchain { get; set; }

        /// <summary>
        /// Gets or sets transactoin identity type
        /// </summary>
        [Required]
        [EnumDataType(typeof(TransactionIdentityType), ErrorMessage = "Invalid value, possible values are: None, Hash, Specific")]
        public TransactionIdentityType IdentityType { get; set; }

        /// <summary>
        /// Gets or sets identity
        /// </summary>
        [Required]
        public string Identity { get; set; }

        /// <summary>
        /// Gets or sets firstsenn date
        /// </summary>
        public DateTime? FirstSeen { get; set; }
    }
}
