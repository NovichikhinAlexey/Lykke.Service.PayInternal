using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Contract.TransferRequest
{
    public class TransferRequestsMessage
    {
        public TransferRequestsMessage()
        {
            TransactionRequests = new List<TransactionRequestMessage>();
        }

        public string TransferId { get; set; }
        public List<TransactionRequestMessage> TransactionRequests { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatus TransferStatus { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TransferStatusError TransferStatusError { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantId { get; set; }

        
       
    }
}
