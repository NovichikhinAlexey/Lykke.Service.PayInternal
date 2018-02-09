using System;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrder
    {
        string Id { get; }

        string MerchantId { get; set; }
        
        string PaymentRequestId { get; set; }

        string AssetPairId { get; set; }
       
        decimal SettlementAmount { get; set; }

        decimal PaymentAmount { get; set; }

        DateTime DueDate { get; set; }

        DateTime CreatedDate { get; set; }    
        
        decimal ExchangeRate { get; set; }
    }
}
