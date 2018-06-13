using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentDirection
    {
        Incoming = 0,

        Outgoing
    }
}
