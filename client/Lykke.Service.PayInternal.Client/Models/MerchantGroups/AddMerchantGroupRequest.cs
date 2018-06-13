using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.MerchantGroups
{
    /// <summary>
    /// New merchant gtoup details
    /// </summary>
    public class AddMerchantGroupRequest
    {
        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets group owner
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
