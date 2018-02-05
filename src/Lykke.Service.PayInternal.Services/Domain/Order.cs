using System;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class Order : IOrder
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string AssetPairId { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
