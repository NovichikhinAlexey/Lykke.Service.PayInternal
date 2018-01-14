using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface ICreateOrderRequest
    {
        string MerchantId { get; set; }

        string InvoiceId { get; set; }

        string AssetPairId { get; set; }

        string InvoiceAssetId { get; set; }

        double InvoiceAmount { get; set; }

        string ExchangeAssetId { get; set; }

        double MarkupPercent { get; set; }

        int MarkupPips { get; set; }

        string SuccessUrl { get; set; }

        string ErrorUrl { get; set; }

        string ProgressUrl { get; set; }

        [CanBeNull] string WalletAddress { get; set; }
    }
}
