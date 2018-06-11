using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.MerchantWallets
{
    /// <summary>
    /// Merchant wallet creation details
    /// </summary>
    public class CreateMerchantWalletModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets wallet network
        /// </summary>
        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: None, Bitcoin, Ethereum")]
        public BlockchainType Network { get; set; }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}
