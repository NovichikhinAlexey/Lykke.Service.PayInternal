namespace Lykke.Service.PayInternal.Client.Models.Refunds
{
    public class RefundRequestModel
    {
        public string MerchantId { get; set; }

        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }
    }
}
