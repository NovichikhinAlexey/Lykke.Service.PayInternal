namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class RefundTransactionResponse
    {
        public string Hash { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
