namespace Lykke.Service.PayInternal.Client.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class ExchangeRequest
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets source merchant wallet id
        /// </summary>
        public string SourceMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination merchant walletd id
        /// </summary>
        public string DestMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        public string DestAssetId { get; set; }
    }
}
