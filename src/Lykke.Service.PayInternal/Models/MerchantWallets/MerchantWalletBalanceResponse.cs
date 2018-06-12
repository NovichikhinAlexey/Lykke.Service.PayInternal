namespace Lykke.Service.PayInternal.Models.MerchantWallets
{
    /// <summary>
    /// Merchant wallet balance details
    /// </summary>
    public class MerchantWalletBalanceResponse
    {
        /// <summary>
        /// Gets or sets merchant wallet id
        /// </summary>
        public string MerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets asset display id
        /// </summary>
        public string AssetDisplayId { get; set; }

        /// <summary>
        /// Gets or sets balance
        /// </summary>
        public decimal Balance { get; set; }
    }
}
