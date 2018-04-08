using Lykke.Service.PayInternal.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class RefundTransactionReponseModel
    {
        public string Hash { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Blockchain { get; set; }
    }
}
