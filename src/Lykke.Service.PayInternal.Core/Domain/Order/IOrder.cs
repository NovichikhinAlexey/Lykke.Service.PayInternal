using System;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrder
    {
        string Id { get; }

        string MerchantId { get; set; }
        
        string PaymentRequestId { get; set; }

        string AssetPairId { get; set; }
       
        double SettlementAmount { get; set; }

        double PaymentAmount { get; set; }

        DateTime DueDate { get; set; }

        DateTime CreatedDate { get; set; }        
    }
}
