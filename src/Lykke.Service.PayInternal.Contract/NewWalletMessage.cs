using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Contract
{
    public class NewWalletMessage
    {
        public string Address { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Blockchain { get; set; }

        public DateTime DueDate { get; set; }
    }
}
