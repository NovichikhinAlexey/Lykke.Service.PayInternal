using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    /// <summary>
    /// Asset general settings response
    /// </summary>
    public class AssetGeneralSettingsResponse
    {
        /// <summary>
        /// Asset display id
        /// </summary>
        public string AssetDisplayId { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Network { get; set; }

        /// <summary>
        /// Get or set if asset is available as payment asset
        /// </summary>
        public bool PaymentAvailable { get; set; }

        /// <summary>
        /// Get or set if asset is available as settlement asset
        /// </summary>
        public bool SettlementAvailable { get; set; }
    }
}
