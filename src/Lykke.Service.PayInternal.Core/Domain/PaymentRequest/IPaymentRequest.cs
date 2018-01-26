using System;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public interface IPaymentRequest
    {
        string Id { get; }

        string MerchantId { get; set; }

        double Amount { get; set; }

        string SettlementAssetId { get; set; }
        
        string PaymentAssetId { get; set; }

        DateTime DueDate { get; set; }
        
        double MarkupPercent { get; set; }

        int MarkupPips { get; set; }
        
        string WalletAddress { get; set; }

        PaymentRequestStatus Status { get; set; }

        double PaidAmount { get; set; }
        
        DateTime? PaidDate { get; set; }
        
        string Error { get; set; }
    }
}
