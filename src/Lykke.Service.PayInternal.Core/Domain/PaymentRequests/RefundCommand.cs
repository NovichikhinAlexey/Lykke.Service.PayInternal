namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class RefundCommand
    {
        public string MerchantId;

        public string PaymentRequestId;

        public string DestinationAddress;
    }
}
