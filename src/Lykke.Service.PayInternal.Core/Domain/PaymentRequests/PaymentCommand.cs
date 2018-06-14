namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class PaymentCommand
    {
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public decimal Amount { get; set; }
    }
}
