using System;

namespace Lykke.Service.PayInternal.Models.Orders
{
    public class OrderModel
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string AssetPairId { get; set; }
        public double SettlementAmount { get; set; }
        public double PaymentAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
