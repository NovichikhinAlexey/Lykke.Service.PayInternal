using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class RefundResponseModel
    {
        public string PaymentRequestId { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public IEnumerable<RefundTransactionReponseModel> Transactions { get; set; }
    }
}
