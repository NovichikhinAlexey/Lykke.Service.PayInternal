using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlockchainType
    {
        None = 0,

        Bitcoin,

        Ethereum,

        EthereumIata
    }
}
