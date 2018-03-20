using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    /// <summary>
    /// Available assets response
    /// </summary>
    public class AvailableAssetsResponse
    {
        /// <summary>
        /// The list of available assets
        /// </summary>
        public IReadOnlyList<string> Assets { get; set; }
    }
}
