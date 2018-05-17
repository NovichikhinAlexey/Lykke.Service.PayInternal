using System;

namespace Lykke.Service.PayInternal.Core.Domain.Orders
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
        public DateTime ExtendedDueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string LwOperationId { get; set; }
    }
}
