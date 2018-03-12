using System;

namespace Lykke.Service.PayInternal.Core.Domain.Refund
{
    public interface IRefund
    {
        string RefundId { get; set; }
        string MerchantId { get; set; }
        string PaymentRequestId { get; set; }
        string SettlementId { get; set; }
        decimal Amount { get; set; }
        DateTime DueDate { get; set; }
    }
}
