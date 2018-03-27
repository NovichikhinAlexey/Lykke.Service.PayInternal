using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentRequestRefund
    {
        public decimal Amount { get; set; }

        public string Address { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime DueDate { get; set; }

        public IEnumerable<PaymentRequestRefundTransaction> Transactions { get; set; }
    }
}
