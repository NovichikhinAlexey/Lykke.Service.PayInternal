using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Groups;

namespace Lykke.Service.PayInternal.Models.MerchantGroups
{
    /// <summary>
    /// Merchant group details
    /// </summary>
    public class MerchantGroupResponse
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
        /// Gets or sets owner id
        /// </summary>
        public string OwnerId { get; set; }

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
