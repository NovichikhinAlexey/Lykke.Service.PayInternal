using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Models;

namespace Lykke.Service.PayInternal
{
    public static class ContractExtensions
    {
        public static CreateOrderResponse ToApiModel(this IOrder src)
        {
            return new CreateOrderResponse
            {
                AssetPairId = src.AssetPairId,
                ExchangeAssetId = src.ExchangeAssetId,
                InvoiceAssetId = src.InvoiceAssetId,
                ExchangeAmount = src.ExchangeAssetId,
                InvoiceAmount = src.InvoiceAmount,
                DueDate = src.DueDate,
                OrderId = src.Id
            };
        }
    }
}
