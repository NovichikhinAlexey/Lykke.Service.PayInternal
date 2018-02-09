using System;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class PaymentRequestOrderModel
    {
        public string Id { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
