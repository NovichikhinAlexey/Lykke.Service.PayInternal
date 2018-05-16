using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Models.Markups
{
    public class MarkupResponse : IMarkupValue
    {
        public string AssetPairId { get; set; }

        public string PriceAssetPairId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PriceMethod PriceMethod { get; set; }

        public decimal DeltaSpread { get; set; }

        public decimal Percent { get; set; }

        public int Pips { get; set; }

        public decimal FixedFee { get; set; }
    }
}
