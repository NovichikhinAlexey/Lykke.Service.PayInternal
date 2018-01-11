using System;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantOrdersService : IMerchantOrdersService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly TimeSpan _orderExpiration;

        public MerchantOrdersService(
            IOrdersRepository ordersRepository,
            TimeSpan orderExpiration)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
            _orderExpiration = orderExpiration;
        }

        public async Task<IOrder> CreateOrder(ICreateOrderRequest request)
        {
            var dueDate = DateTime.UtcNow.Add(_orderExpiration);

            return await _ordersRepository.SaveAsync(new OrderEntity
            {
                ExchangeAssetId = request.ExchangeAssetId,
                AssetPairId = request.AssetPairId,
                DueDate = dueDate,
                ExchangeAmount = request.ExchangeAmount,
                InvoiceAmount = request.InvoiceAmount,
                InvoiceAssetId = request.InvoiceAssetId,
                InvoiceId = request.InvoiceId,
                MarkupFixedFee = request.MarkupFixedFee,
                MarkupPercent = request.MarkupPercent,
                MarkupPips = request.MarkupPips,
                MerchantId = request.MerchantId
            });
        }
    }
}
