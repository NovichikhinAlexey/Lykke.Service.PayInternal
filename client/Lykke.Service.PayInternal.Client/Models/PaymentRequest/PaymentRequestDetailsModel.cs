using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class PaymentRequestDetailsModel
    {
        public PaymentRequestDetailsModel()
        {
            Transactions= new List<PaymentRequestTransactionModel>();
        }
        
        public string Id { get; set; }
        
        public string MerchantId { get; set; }
        
        public double Amount { get; set; }
        
        public string SettlementAssetId { get; set; }
        
        public string PaymentAssetId { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public double MarkupPercent { get; set; }
        
        public int MarkupPips { get; set; }
        
        public string WalletAddress { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestStatus Status { get; set; }
        
        public double PaidAmount { get; set; }
        
        public DateTime? PaidDate { get; set; }
        
        public string Error { get; set; }

        public PaymentRequestOrderModel Order { get; set; }
        
        public List<PaymentRequestTransactionModel> Transactions { get; set; }
    }
}
