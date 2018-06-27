namespace Lykke.Service.PayInternal.Client.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class PreExchangeRequest
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        public string DestAssetId { get; set; }
    }
}
