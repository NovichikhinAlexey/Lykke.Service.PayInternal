using System;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class PaymentRequest : IPaymentRequest
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string SettlementAssetId { get; set; }
        public string PaymentAssetId { get; set; }
        public DateTime DueDate { get; set; }
        public double MarkupPercent { get; set; }
        public int MarkupPips { get; set; }
        public string WalletAddress { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Error { get; set; }
    }
}
