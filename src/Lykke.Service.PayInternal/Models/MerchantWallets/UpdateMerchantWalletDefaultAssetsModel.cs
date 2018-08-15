using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Validation;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.MerchantWallets
{
    /// <summary>
    /// Update merchant wallet default assets request details
    /// </summary>
    public class UpdateMerchantWalletDefaultAssetsModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        [MerchantExists]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets wallet network
        /// </summary>
        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: Bitcoin, Ethereum, EthereumIata")]
        public BlockchainType? Network { get; set; }

        /// <summary>
        /// Gets or sets wallet address
        /// </summary>
        [Required]
        [RowKey]
        public string WalletAddress { get; set; }

        /// <summary>
        /// Gets or sets list of assets for which wallet will be default for incoming payments
        /// </summary>
        public IList<string> IncomingPaymentDefaults { get; set; }

        /// <summary>
        /// Gets or sets list of assets for which wallet will be default for incoming payments
        /// </summary>
        public IList<string> OutgoingPaymentDefaults { get; set; }
    }
}
