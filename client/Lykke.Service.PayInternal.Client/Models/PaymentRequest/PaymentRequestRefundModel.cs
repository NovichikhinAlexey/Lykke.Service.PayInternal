using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class PaymentRequestRefundModel
    {
        public decimal Amount { get; set; }

        public string Address { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime DueDate { get; set; }

        public List<PaymentRequestRefundTransactionModel> Transactions { get; set; }
    }
}
