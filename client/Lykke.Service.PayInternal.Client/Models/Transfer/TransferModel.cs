using System;
using Newtonsoft.Json;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    public class TransferModel
    {
        [JsonProperty(PropertyName = "clientPubKey")]
        public string ClientPubKey { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public Decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "asset")]
        public string Asset { get; set; }

        [JsonProperty(PropertyName = "clientPrevPrivateKey")]
        public string ClientPrevPrivateKey { get; set; }

        [JsonProperty(PropertyName = "requiredOperation")]
        public bool? RequiredOperation { get; set; }

        [JsonProperty(PropertyName = "transferId")]
        public Guid? TransferId { get; set; }

        public TransferModel()
        {
        }

        public TransferModel(string clientPubKey = null, Decimal? amount = null, string asset = null, string clientPrevPrivateKey = null, bool? requiredOperation = null, Guid? transferId = null)
        {
            this.ClientPubKey = clientPubKey;
            this.Amount = amount;
            this.Asset = asset;
            this.ClientPrevPrivateKey = clientPrevPrivateKey;
            this.RequiredOperation = requiredOperation;
            this.TransferId = transferId;
        }
    }
}

