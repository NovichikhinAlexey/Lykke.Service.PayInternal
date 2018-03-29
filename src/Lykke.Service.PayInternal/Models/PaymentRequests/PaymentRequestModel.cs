using System;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class PaymentRequestModel
    {
        public string Id { get; set; }

        public string MerchantId { get; set; }

        public decimal Amount { get; set; }

        public string OrderId { get; set; }

        public string SettlementAssetId { get; set; }

        public string PaymentAssetId { get; set; }

        public DateTime DueDate { get; set; }

        public double MarkupPercent { get; set; }

        public int MarkupPips { get; set; }

        public double MarkupFixedFee { get; set; }

        public string WalletAddress { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestStatus Status { get; set; }

        public decimal PaidAmount { get; set; }

        public DateTime? PaidDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestErrorType Error { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
