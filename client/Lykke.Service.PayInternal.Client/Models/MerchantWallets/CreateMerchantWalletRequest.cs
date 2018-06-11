namespace Lykke.Service.PayInternal.Client.Models.MerchantWallets
{
    /// <summary>
    /// Merchant wallet creation details
    /// </summary>
    public class CreateMerchantWalletRequest
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets wallet network
        /// </summary>
        public BlockchainType Network { get; set; }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}
