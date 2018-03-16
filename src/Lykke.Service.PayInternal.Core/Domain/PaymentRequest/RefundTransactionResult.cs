using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public class RefundTransactionResult
    {
        public string Hash { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        [CanBeNull] public string Blockchain { get; set; }
    }
}
