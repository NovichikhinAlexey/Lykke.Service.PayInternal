using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    public class PaymentRequestTransaction
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime FirstSeen { get; set; }
        public IReadOnlyList<string> SourceWalletAddresses { get; set; }
    }
}
