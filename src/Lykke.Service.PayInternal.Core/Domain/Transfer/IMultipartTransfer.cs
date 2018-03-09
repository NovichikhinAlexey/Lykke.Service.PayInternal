using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface IMultipartTransfer
    {
        string TransferId { get; set; }
        string PaymentRequestId { get; set; }
        string MerchantId { get; set; }
        DateTime CreationDate { get; set; }
        string AssetId { get; set; }
        int FeeRate { get; set; }
        decimal FixedFee { get; set; }
    }
}
