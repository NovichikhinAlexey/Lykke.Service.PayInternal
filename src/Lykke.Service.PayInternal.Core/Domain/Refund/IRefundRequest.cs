namespace Lykke.Service.PayInternal.Core.Domain.Refund
{
    public interface IRefundRequest
    {
        string MerchantId { get; set; }
        string SourceAddress { get; set; }
        string DestinationAddress { get; set; }
    }
}
