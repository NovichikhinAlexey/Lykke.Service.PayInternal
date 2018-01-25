using System;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface ICreateOrderResponse
    {
        string OrderId { get; set; }

        DateTime DueDate { get; set; }

        string AssetPairId { get; set; }

        string InvoiceAssetId { get; set; }

        double InvoiceAmount { get; set; }

        string ExchangeAssetId { get; set; }

        double ExchangeAmount { get; set; }

        string WalletAddress { get; set; }
    }
}
