using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICalculationService _calculationService;
        private readonly IMarkupService _markupService;
        private readonly ILog _log;
        private readonly OrderExpirationPeriodsSettings _orderExpirationPeriods;

        public OrderService(
            IOrderRepository orderRepository,
            IAssetsLocalCache assetsLocalCache,
            IMerchantRepository merchantRepository,
            ICalculationService calculationService,
            ILog log,
            OrderExpirationPeriodsSettings orderExpirationPeriods, 
            IMarkupService markupService)
        {
            _orderRepository = orderRepository;
            _assetsLocalCache = assetsLocalCache;
            _merchantRepository = merchantRepository;
            _calculationService = calculationService;
            _log = log;
            _orderExpirationPeriods = orderExpirationPeriods;
            _markupService = markupService;
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, string orderId)
        {
            if (string.IsNullOrEmpty(paymentRequestId) || string.IsNullOrEmpty(orderId))
                return null;

            return await _orderRepository.GetAsync(paymentRequestId, orderId);
        }

        public async Task<IOrder> GetActualAsync(string paymentRequestId, DateTime date)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                return null;

            IReadOnlyList<IOrder> orders = await _orderRepository.GetAsync(paymentRequestId);

            return orders
                .Where(o => date < o.ExtendedDueDate)
                .OrderBy(o => o.ExtendedDueDate)
                .FirstOrDefault();
        }

        public async Task<IOrder> GetLatestOrCreateAsync(IPaymentRequest paymentRequest, bool force = false)
        {
            IReadOnlyList<IOrder> orders = await _orderRepository.GetAsync(paymentRequest.Id);

            IOrder latestOrder = orders.OrderByDescending(x => x.ExtendedDueDate).FirstOrDefault();

            var now = DateTime.UtcNow;

            if (latestOrder != null)
            {
                if (now < latestOrder.DueDate)
                    return latestOrder;

                if (now < latestOrder.ExtendedDueDate && !force)
                    return latestOrder;
            }

            IMerchant merchant = await _merchantRepository.GetAsync(paymentRequest.MerchantId);

            if (merchant == null)
                throw new MerchantNotFoundException(paymentRequest.MerchantId);

            AssetPair assetPair =
                await _assetsLocalCache.GetAssetPairAsync(paymentRequest.PaymentAssetId, paymentRequest.SettlementAssetId);

            RequestMarkup requestMarkup = Mapper.Map<RequestMarkup>(paymentRequest);

            IMarkup merchantMarkup;

            try
            {
                merchantMarkup = await _markupService.ResolveAsync(merchant.Id, assetPair.Id);
            }
            catch (MarkupNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(OrderService), nameof(GetLatestOrCreateAsync), new
                {
                    ex.MerchantId,
                    ex.AssetPairId
                }.ToJson(), ex);

                throw;
            }

            decimal paymentAmount = await _calculationService
                .GetAmountAsync(assetPair.Id, paymentRequest.Amount, requestMarkup, merchantMarkup);

            decimal rate = await _calculationService.GetRateAsync(assetPair.Id, requestMarkup.Percent,
                requestMarkup.Pips, merchantMarkup);

            var order = new Order
            {
                MerchantId = paymentRequest.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                AssetPairId = assetPair.Id,
                SettlementAmount = paymentRequest.Amount,
                PaymentAmount = paymentAmount,
                DueDate = now.Add(_orderExpirationPeriods.Primary),
                ExtendedDueDate = now.Add(_orderExpirationPeriods.Extended),
                CreatedDate = now,
                ExchangeRate = rate
            };

            IOrder createdOrder = await _orderRepository.InsertAsync(order);

            await _log.WriteInfoAsync(nameof(OrderService), nameof(GetLatestOrCreateAsync), order.ToJson(),
                "Order created.");

            return createdOrder;
        }
    }
}
