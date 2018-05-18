using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Contract
{
    public class NewTransactionMessage
    {
        public string Id { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string BlockId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Blockchain { get; set; }

        public int Confirmations { get; set; }

        public DateTime DueDate { get; set; }
    }
}
