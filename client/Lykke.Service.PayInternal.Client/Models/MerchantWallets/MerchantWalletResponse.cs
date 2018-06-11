using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.MerchantWallets
{
    /// <summary>
    /// Merchant wallet details
    /// </summary>
    public class MerchantWalletResponse
    {
        /// <summary>
        /// Gets or sets merchant wallet id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets network
        /// </summary>
        public BlockchainType Network { get; set; }

        /// <summary>
        /// Gets or sets wallet address
        /// </summary>
        public string WalletAddress { get; set; }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets created date
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets list of assets for which wallet will be default for incoming payments
        /// </summary>
        public IList<string> IncomingPaymentDefaults { get; set; }

        /// <summary>
        /// Gets or sets list of assets for which wallet will be default for outgoing payments
        /// </summary>
        public IList<string> OutgoingPaymentDefaults { get; set; }
    }
}
