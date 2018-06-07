using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    /// <summary>
    /// Request to update asset general settings
    /// </summary>
    public class UpdateAssetGeneralSettingsRequest
    {
        /// <summary>
        /// Asset display id
        /// </summary>
        [Required]
        public string AssetDisplayId { get; set; }

        /// <summary>
        /// Get or set if asset is available as payment asset
        /// </summary>
        [Required]
        public bool PaymentAvailable { get; set; }

        /// <summary>
        /// Get or set if asset is available as settlement asset
        /// </summary>
        [Required]
        public bool SettlementAvailable { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Network { get; set; }
    }
}
