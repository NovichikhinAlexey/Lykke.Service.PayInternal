using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class PaymentRequestTransactionModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime FirstSeen { get; set; }
        public string Url { get; set; }
        public string RefundUrl { get; set; }
        public IReadOnlyList<string> SourceWalletAddresses { get; set; }
    }
}
