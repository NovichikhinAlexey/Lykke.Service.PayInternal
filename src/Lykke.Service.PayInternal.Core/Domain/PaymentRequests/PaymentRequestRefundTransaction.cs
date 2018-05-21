using System;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentRequestRefundTransaction
    {
        public string Hash { get; set; }

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public int NumberOfConfirmations { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
