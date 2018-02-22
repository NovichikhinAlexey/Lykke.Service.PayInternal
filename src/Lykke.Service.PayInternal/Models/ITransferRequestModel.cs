using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public interface ITransferRequestModel
    {
        string MerchantId { get; set; }
        TransferFeePayerEnum FeePayer { get; set; }
        string CheckAmountsValidity();
        ITransferRequest ToTransferRequest();
    }
}
