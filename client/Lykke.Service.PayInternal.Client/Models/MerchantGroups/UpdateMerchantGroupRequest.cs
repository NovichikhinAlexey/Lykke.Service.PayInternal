using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.MerchantGroups
{
    /// <summary>
    /// Merchant group update details
    /// </summary>
    public class UpdateMerchantGroupRequest
    {
        /// <summary>
        /// Gets or sets group id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets list of merchants
        /// </summary>
        public IEnumerable<string> Merchants { get; set; }

        /// <summary>
        /// Gets or sets merchant group use
        /// </summary>
        public MerchantGroupUse MerchantGroupUse { get; set; }
    }
}
