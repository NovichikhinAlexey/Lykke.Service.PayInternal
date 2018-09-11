using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public interface IPaymentRequest
    {
        string Id { get; }

        string MerchantId { get; set; }

        string ExternalOrderId { get; set; }

        string OrderId { get; set; }
        
        decimal Amount { get; set; }

        string SettlementAssetId { get; set; }
        
        string PaymentAssetId { get; set; }

        DateTime DueDate { get; set; }
        
        decimal MarkupPercent { get; set; }

        int MarkupPips { get; set; }

        decimal MarkupFixedFee { get; set; }
        
        string WalletAddress { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        PaymentRequestStatus Status { get; set; }

        decimal PaidAmount { get; set; }
        
        DateTime? PaidDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        PaymentRequestProcessingError ProcessingError { get; set; }

        DateTime Timestamp { get; set; }

        bool StatusValidForRefund();

        bool StatusValidForPastDueTransition();

        bool StatusValidForSettlement();

        string Initiator { get; set; }

        string SettlementErrorDescription { get; set; }
    }
}
