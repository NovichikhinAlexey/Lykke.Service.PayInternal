using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.Groups
{
    public enum MerchantGroupUse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Supervising
    }
}
