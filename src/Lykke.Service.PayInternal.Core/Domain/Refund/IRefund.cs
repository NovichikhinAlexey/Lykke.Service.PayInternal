using System;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Core.Domain.Refund
{
    public interface IRefund
    {
        string RefundId { get; set; }
        string MerchantId { get; set; }
        string PaymentRequestId { get; set; }
        PaymentRequestStatus PaymentRequestStatus { get; set; } 
        string SettlementId { get; set; }
        decimal Amount { get; set; }
        DateTime DueDate { get; set; }
    }
}
