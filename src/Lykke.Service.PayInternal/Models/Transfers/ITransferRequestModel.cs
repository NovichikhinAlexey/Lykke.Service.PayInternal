using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models.Transfers
{
    public interface ITransferRequestModel
    {
        string PaymentRequestId { get; set; }
        string MerchantId { get; set; }
        string AssetId { get; set; }
        int FeeRate { get; set; }
        decimal FixedFee { get; set; }

        TransferRequestModelValidationResult CheckAmountsValidity();
        IMultipartTransfer ToDomain();
    }

    public enum TransferRequestModelValidationResult
    {
        Valid,
        NegativeSourceAmount,
        NegatveDestinationAmount,
        NotEqualSourceDestAmounts,
        NegativeFeeRate,
        NegativeFixedFee
    }
}
