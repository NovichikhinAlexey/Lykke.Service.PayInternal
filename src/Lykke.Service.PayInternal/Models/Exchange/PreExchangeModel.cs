using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class PreExchangeModel
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        [MerchantExists]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        [Required]
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string DestAssetId { get; set; }
    }
}
