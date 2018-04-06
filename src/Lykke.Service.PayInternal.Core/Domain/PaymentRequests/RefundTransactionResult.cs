namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class RefundTransactionResult
    {
        public string Hash { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
