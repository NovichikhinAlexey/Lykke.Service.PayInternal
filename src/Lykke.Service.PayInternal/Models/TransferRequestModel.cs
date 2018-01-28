using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferRequestModel : ITransferRequest
    {
        public TransferRequestModel()
        {
            Amount = 0;
            Currency = "BTC";
        }
        public string MerchantId { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
