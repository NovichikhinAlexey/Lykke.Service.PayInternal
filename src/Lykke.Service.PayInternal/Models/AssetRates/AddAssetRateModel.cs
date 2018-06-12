using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.AssetRates
{
    /// <summary>
    /// Asset rate
    /// </summary>
    public class AddAssetRateModel
    {
        /// <summary>
        /// Gets or sets base asset
        /// </summary>
        [Required]
        public string BaseAssetId { get; set; }

        /// <summary>
        /// Gets pr sets quoting asset
        /// </summary>
        [Required]
        public string QuotingAssetId { get; set; }

        /// <summary>
        /// Gets or sets bid price
        /// </summary>
        [Required]
        public decimal BidPrice { get; set; }

        /// <summary>
        /// Gets or sets ask price
        /// </summary>
        [Required]
        public decimal AskPrice { get; set; }
    }
}
