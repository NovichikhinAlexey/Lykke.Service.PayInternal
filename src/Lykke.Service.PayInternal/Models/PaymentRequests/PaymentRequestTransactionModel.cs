using System;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class PaymentRequestTransactionModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}
