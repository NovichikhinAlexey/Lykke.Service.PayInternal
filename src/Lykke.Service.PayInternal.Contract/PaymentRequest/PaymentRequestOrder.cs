using System;

namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    public class PaymentRequestOrder
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
