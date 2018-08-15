namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentResult
    {
        public string PaymentRequestId { get; set; }

        public string PaymentRequestWalletAddress { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }
    }
}
