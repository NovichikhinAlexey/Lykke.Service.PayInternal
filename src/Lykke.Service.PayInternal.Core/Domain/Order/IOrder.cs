using System;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrder
    {
        string Id { get; }

        string MerchantId { get; set; }

        string InvoiceId { get; set; }

        string AssetPairId { get; set; }

        string InvoiceAssetId { get; set; }

        double InvoiceAmount { get; set; }

        string ExchangeAssetId { get; set; }

        double ExchangeAmount { get; set; }

        DateTime DueDate { get; set; }

        double MarkupPercent { get; set; }

        int MarkupPips { get; set; }

        string SuccessUrl { get; set; }

        string ErrorUrl { get; set; }

        string ProgressUrl { get; set; }

        string WalletAddress { get; set; }
    }
}
