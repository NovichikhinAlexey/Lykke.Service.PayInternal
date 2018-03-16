namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class RefundRequestModel
    {
        public string MerchantId { get; set; }

        public string PaymentRequestId { get; set; }

        public string DestinationAddress { get; set; }

        public string CallbackUrl { get; set; }
    }
}
