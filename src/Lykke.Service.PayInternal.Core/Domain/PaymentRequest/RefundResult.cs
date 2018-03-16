using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public class RefundResult
    {
        public string PaymentRequestId { get; set; }

        public string PaymentRequestWalletAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public IEnumerable<RefundTransactionResult> Transactions { get; set; }

        public DateTime DueDate { get; set; }
    }
}
