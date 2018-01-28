using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IRatesCalculationService _ratesCalculationService;
        private readonly ILog _log;
        private readonly TimeSpan _orderExpiration;

        public OrderService(
            IOrderRepository orderRepository,
            IAssetsLocalCache assetsLocalCache,
            IMerchantRepository merchantRepository,
            IRatesCalculationService ratesCalculationService,
            ILog log,
            TimeSpan orderExpiration)
        {
            _orderRepository = orderRepository;
            _assetsLocalCache = assetsLocalCache;
            _merchantRepository = merchantRepository;
            _ratesCalculationService = ratesCalculationService;
            _log = log;
            _orderExpiration = orderExpiration;
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, string orderId)
        {
            return await _orderRepository.GetAsync(paymentRequestId, orderId);
        }

        public async Task<IOrder> GetLatestOrCreateAsync(IPaymentRequest paymentRequest)
        {
            IReadOnlyList<IOrder> orders = await _orderRepository.GetAsync(paymentRequest.Id);
           
            IOrder latestOrder = orders.OrderByDescending(x => x.DueDate).FirstOrDefault();
            
            if (latestOrder?.DueDate > DateTime.UtcNow) 
                return latestOrder;

            IMerchant merchant = await _merchantRepository.GetAsync(paymentRequest.MerchantId);

            if (merchant == null)
                throw new MerchantNotFoundException(paymentRequest.MerchantId);

            AssetPair assetPair =
                await _assetsLocalCache.GetAssetPairAsync(paymentRequest.PaymentAssetId, paymentRequest.SettlementAssetId);
            
            var merchantMarkup = new MerchantMarkup
            {
                LpPercent = merchant.LpMarkupPercent,
                DeltaSpread = merchant.DeltaSpread,
                LpPips = merchant.LpMarkupPips
            };

            var requestMarkup = new RequestMarkup
            {
                Percent = paymentRequest.MarkupPercent,
                Pips = paymentRequest.MarkupPips,
                FixedFee = merchant.MarkupFixedFee
            };

            double paymentAmount = await _ratesCalculationService
                .GetAmount(assetPair.Id, paymentRequest.Amount, requestMarkup, merchantMarkup);

            var order = new Order
            {
                MerchantId = paymentRequest.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                AssetPairId = assetPair.Id,
                SettlementAmount = paymentRequest.Amount,
                PaymentAmount = paymentAmount,
                DueDate = DateTime.UtcNow.Add(_orderExpiration),
                CreatedDate = DateTime.UtcNow
            };

            IOrder createdOrder = await _orderRepository.InsertAsync(order);

            await _log.WriteInfoAsync(nameof(OrderService), nameof(GetLatestOrCreateAsync), order.ToJson(), "Order created.");
            
            return createdOrder;
        }
    }
}
