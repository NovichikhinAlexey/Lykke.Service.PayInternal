using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class RefundResponse
    {
        public string PaymentRequestId { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public IEnumerable<RefundTransactionResponse> Transactions { get; set; }
    }
}
