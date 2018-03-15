using System;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Core.Domain.Refund
{
    public class RefundResult
    {
        public string MerchantId { get; set; }

        public string PaymentRequestId { get; set; }

        public PaymentRequestStatus PaymentRequestStatus { get; set; }

        public DateTime DueDate { get; set; }

        public decimal Amount { get; set; }
    }
}
