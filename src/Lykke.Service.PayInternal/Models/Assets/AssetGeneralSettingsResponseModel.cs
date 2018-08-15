using Lykke.Service.PayInternal.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class AssetGeneralSettingsResponseModel
    {
        public string AssetDisplayId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Network { get; set; }

        public bool PaymentAvailable { get; set; }

        public bool SettlementAvailable { get; set; }

        public bool AutoSettle { get; set; }
    }
}
