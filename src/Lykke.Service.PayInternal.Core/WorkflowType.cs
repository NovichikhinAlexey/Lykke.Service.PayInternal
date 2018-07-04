using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WorkflowType
    {
        LykkePay = 0,
        Airlines,
    }
}
