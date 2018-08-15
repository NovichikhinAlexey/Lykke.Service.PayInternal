namespace Lykke.Service.PayInternal.Client.Models.AssetRates
{
    /// <summary>
    /// Asset rate
    /// </summary>
    public class AddAssetRateRequest
    {
        /// <summary>
        /// Gets or sets base asset
        /// </summary>
        public string BaseAssetId { get; set; }

        /// <summary>
        /// Gets or sets quoting asset
        /// </summary>
        public string QuotingAssetId { get; set; }

        /// <summary>
        /// Gets or sets bid price
        /// </summary>
        public decimal BidPrice { get; set; }

        /// <summary>
        /// Gets or sets ask price
        /// </summary>
        public decimal AskPrice { get; set; }
    }
}
