namespace Lykke.Service.PayInternal.Client.Models.Cashout
{
    /// <summary>
    /// Cashout request details
    /// </summary>
    public class CashoutRequest
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets employee email
        /// </summary>
        public string EmployeeEmail { get; set; }

        /// <summary>
        /// Gets or sets source merchant wallet id
        /// </summary>
        public string SourceMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets desired asset
        /// </summary>
        public string DesiredAsset { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        public decimal SourceAmount { get; set; }
    }
}
