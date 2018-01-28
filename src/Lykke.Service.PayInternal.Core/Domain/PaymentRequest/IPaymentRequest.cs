using System;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public interface IPaymentRequest
    {
        string Id { get; }

        string MerchantId { get; set; }

        string OrderId { get; set; }
        
        decimal Amount { get; set; }

        string SettlementAssetId { get; set; }
        
        string PaymentAssetId { get; set; }

        DateTime DueDate { get; set; }
        
        double MarkupPercent { get; set; }

        int MarkupPips { get; set; }
        
        string WalletAddress { get; set; }

        PaymentRequestStatus Status { get; set; }

        decimal PaidAmount { get; set; }
        
        DateTime? PaidDate { get; set; }
        
        string Error { get; set; }
    }
}
