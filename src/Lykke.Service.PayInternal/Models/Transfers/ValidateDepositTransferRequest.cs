using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Transfers
{
    /// <summary>
    /// Request details to validate deposit transfer from temporary addresses
    /// </summary>
    public class ValidateDepositTransferRequest : IBlockchainTypeHolder
    {
        /// <summary>
        /// Gets or sets blockchain type
        /// </summary>
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: Bitcoin, Ethereum")]
        [Required]
        public BlockchainType Blockchain { get; set; }       
        
        /// <summary>
        /// Gets or sets blockchain wallet address
        /// </summary>
        [Required]
        public string WalletAddress { get; set; }

        /// <summary>
        /// Gets or sets deposit transfer amount
        /// </summary>
        [DecimalGreaterThanZero]
        public decimal TransferAmount { get; set; }
    }
}
