using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public class BlockchainWallet
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Blockchain { get; set; }

        public string AssetId { get; set; }

        public string Address { get; set; }

        public string Data { get; set; }
    }
}
