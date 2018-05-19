using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;
using Order = Lykke.Service.PayInternal.Services.Domain.Order;

namespace Lykke.Service.PayInternal.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICalculationService _calculationService;
        private readonly IMarkupService _markupService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;
        private readonly OrderExpirationPeriodsSettings _orderExpirationPeriods;

        public OrderService(
            IOrderRepository orderRepository,
            IMerchantRepository merchantRepository,
            ICalculationService calculationService,
            ILog log,
            OrderExpirationPeriodsSettings orderExpirationPeriods, 
            IMarkupService markupService,
            ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _orderRepository = orderRepository;
            _merchantRepository = merchantRepository;
            _calculationService = calculationService;
            _log = log;
            _orderExpirationPeriods = orderExpirationPeriods;
            _markupService = markupService;
            _lykkeAssetsResolver = lykkeAssetsResolver;
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, string orderId)
        {
            if (string.IsNullOrEmpty(paymentRequestId) || string.IsNullOrEmpty(orderId))
                return null;

            return await _orderRepository.GetByPaymentRequestAsync(paymentRequestId, orderId);
        }

        public async Task<IOrder> GetActualAsync(string paymentRequestId, DateTime date)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                return null;

            IReadOnlyList<IOrder> orders = await _orderRepository.GetByPaymentRequestAsync(paymentRequestId);

            return orders
                .Where(o => date < o.ExtendedDueDate)
                .OrderBy(o => o.ExtendedDueDate)
                .FirstOrDefault();
        }

        public async Task<IOrder> GetLatestOrCreateAsync(IPaymentRequest paymentRequest, bool force = false)
        {
            IReadOnlyList<IOrder> orders = await _orderRepository.GetByPaymentRequestAsync(paymentRequest.Id);

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

            string lykkePaymentAssetId = await _lykkeAssetsResolver.GetLykkeId(paymentRequest.PaymentAssetId);

            string lykkeSettlementAssetId = await _lykkeAssetsResolver.GetLykkeId(paymentRequest.SettlementAssetId);

            string assetPairId = $"{paymentRequest.PaymentAssetId}{paymentRequest.SettlementAssetId}";

            RequestMarkup requestMarkup = Mapper.Map<RequestMarkup>(paymentRequest);

            IMarkup merchantMarkup;

            try
            {
                merchantMarkup = await _markupService.ResolveAsync(merchant.Id, assetPairId);
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

            decimal paymentAmount, rate;

            try
            {
                paymentAmount = await _calculationService.GetAmountAsync(lykkePaymentAssetId,
                    lykkeSettlementAssetId, paymentRequest.Amount, requestMarkup, merchantMarkup);

                rate = await _calculationService.GetRateAsync(lykkePaymentAssetId, lykkeSettlementAssetId,
                    requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);
            }
            catch (MarketPriceZeroException priceZeroEx)
            {
                _log.WriteError(nameof(GetLatestOrCreateAsync), new { priceZeroEx.PriceType }, priceZeroEx);

                throw;
            }
            catch (UnexpectedAssetPairPriceMethodException assetPairEx)
            {
                _log.WriteError(nameof(GetLatestOrCreateAsync), new {assetPairEx.PriceMethod}, assetPairEx);

                throw;
            }

            var order = new Order
            {
                MerchantId = paymentRequest.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                AssetPairId = assetPairId,
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

        public Task<IOrder> GetByLykkeOperationAsync(string operationId)
        {
            return _orderRepository.GetByLykkeOperationAsync(operationId);
        }
    }
}
