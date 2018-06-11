using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.MerchantWallets
{
    /// <summary>
    /// Update merchant wallet default assets request details
    /// </summary>
    public class UpdateMerchantWalletDefaultAssetsRequest
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
        /// Gets or sets wallet address
        /// </summary>
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
