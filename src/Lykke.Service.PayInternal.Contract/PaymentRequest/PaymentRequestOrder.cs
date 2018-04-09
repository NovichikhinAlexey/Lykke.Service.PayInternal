using System;

namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    public class PaymentRequestOrder
    {
        public string Id { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ExtendedDueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
