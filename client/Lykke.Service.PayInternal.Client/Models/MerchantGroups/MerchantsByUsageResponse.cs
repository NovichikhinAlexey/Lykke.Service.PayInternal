using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.MerchantGroups
{
    /// <summary>
    /// Merchants by usage response details
    /// </summary>
    public class MerchantsByUsageResponse
    {
        /// <summary>
        /// Gets or sets list of merchants
        /// </summary>
        public IEnumerable<string> Merchants { get; set; }
    }
}
