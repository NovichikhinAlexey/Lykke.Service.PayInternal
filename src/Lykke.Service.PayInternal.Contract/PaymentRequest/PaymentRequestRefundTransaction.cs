using System;

namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    public class PaymentRequestRefundTransaction
    {
        public string Hash { get; set; }

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public int NumberOfConfirmations { get; set; }

        public string Url { get; set; }
    }
}
