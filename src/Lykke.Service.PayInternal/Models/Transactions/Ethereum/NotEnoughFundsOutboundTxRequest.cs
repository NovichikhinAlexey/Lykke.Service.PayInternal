using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models.Transactions.Ethereum
{
    /// <summary>
    /// Not enough funds ountbound transaction request details
    /// </summary>
    public class NotEnoughFundsOutboundTxRequest : IBlockchainTypeHolder
    {
        /// <summary>
        /// Gets or sets identity type
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
        /// Gets or sets operation id
        /// </summary>
        public string OperationId { get; set; }

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
        /// Gets or sets amount
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

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
    }
}
