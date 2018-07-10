using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceMethod
    {
        None = 0,

        Direct,

        Reverse
    }
}
