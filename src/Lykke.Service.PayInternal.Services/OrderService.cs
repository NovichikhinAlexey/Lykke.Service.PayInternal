using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Markup;
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
        private readonly ICalculationService _calculationService;
        private readonly IMarkupService _markupService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;
        private readonly OrderExpirationPeriodsSettings _orderExpirationPeriods;

        public OrderService(
            [NotNull] IOrderRepository orderRepository,
            [NotNull] ICalculationService calculationService,
            [NotNull] ILogFactory logFactory,
            [NotNull] OrderExpirationPeriodsSettings orderExpirationPeriods,
            [NotNull] IMarkupService markupService,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _orderRepository = orderRepository;
            _calculationService = calculationService;
            _log = logFactory.CreateLog(this);
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

        public async Task<IOrder> GetActualAsync(string paymentRequestId, DateTime date, decimal? paid = null)
        {
            if (string.IsNullOrEmpty(paymentRequestId))
                return null;

            IReadOnlyList<IOrder> allOrders = await _orderRepository.GetByPaymentRequestAsync(paymentRequestId);
            
            if (paid.HasValue)
            {
                _log.Info(nameof(GetActualAsync), $"paymentRequestId = [{paymentRequestId}]");
                _log.Info(nameof(GetActualAsync), $"date = [{date.ToString(CultureInfo.InvariantCulture)}]");
                _log.Info(nameof(GetActualAsync), $"paid = [{paid.ToString()}]");
                
                _log.Info(nameof(GetActualAsync), "All orders", allOrders.ToJson());
                
                var orderMatchByAmount = allOrders
                    .Where(o => date < o.ExtendedDueDate && decimal.Equals(o.PaymentAmount, paid.Value))
                    .OrderBy(o => o.ExtendedDueDate)
                    .FirstOrDefault();

                if (orderMatchByAmount != null)
                {
                    _log.Info(nameof(GetActualAsync), "Order matched by amount", orderMatchByAmount.ToJson());
                    return orderMatchByAmount;
                }
            }

            _log.Info(nameof(GetActualAsync), "No orders to match by amount only match by date will be used");
            
            return allOrders
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

            RequestMarkup requestMarkup = Mapper.Map<RequestMarkup>(paymentRequest);

            var paymentInfo = await GetPaymentInfoAsync(paymentRequest.SettlementAssetId, paymentRequest.PaymentAssetId, paymentRequest.Amount, paymentRequest.MerchantId, requestMarkup);

            var order = new Order
            {
                MerchantId = paymentRequest.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                AssetPairId = paymentInfo.AssetPairId,
                SettlementAmount = paymentRequest.Amount,
                PaymentAmount = paymentInfo.PaymentAmount,
                DueDate = now.Add(_orderExpirationPeriods.Primary),
                ExtendedDueDate = now.Add(_orderExpirationPeriods.Extended),
                CreatedDate = now,
                ExchangeRate = paymentInfo.Rate
            };

            IOrder createdOrder = await _orderRepository.InsertAsync(order);

            _log.Info("Order created", order.ToJson());

            return createdOrder;
        }

        public virtual async Task<(string AssetPairId, decimal PaymentAmount, decimal Rate)> GetPaymentInfoAsync(string settlementAssetId, string paymentAssetId, decimal amount, string merchantId, IRequestMarkup requestMarkup)
        {
            string lykkePaymentAssetId = await _lykkeAssetsResolver.GetLykkeId(paymentAssetId);

            string lykkeSettlementAssetId = await _lykkeAssetsResolver.GetLykkeId(settlementAssetId);

            string assetPairId = $"{paymentAssetId}{settlementAssetId}";

            IMarkup merchantMarkup;

            try
            {
                merchantMarkup = await _markupService.ResolveAsync(merchantId, assetPairId); 
            }
            catch (MarkupNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.AssetPairId
                });

                throw;
            }

            decimal paymentAmount, rate;

            try
            {
                paymentAmount = await _calculationService.GetAmountAsync(lykkePaymentAssetId,
                    lykkeSettlementAssetId, amount, requestMarkup, merchantMarkup);

                rate = await _calculationService.GetRateAsync(lykkePaymentAssetId, lykkeSettlementAssetId,
                    requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);
            }
            catch (MarketPriceZeroException e)
            {
                _log.ErrorWithDetails(e, new { e.PriceType });

                throw;
            }
            catch (UnexpectedAssetPairPriceMethodException e)
            {
                _log.ErrorWithDetails(e, new { e.PriceMethod });

                throw;
            }
            catch (ZeroRateException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    lykkePaymentAssetId,
                    lykkeSettlementAssetId,
                    requestMarkup.Percent,
                    requestMarkup.Pips,
                    merchantMarkup
                });

                throw;
            }

            return (assetPairId, paymentAmount, rate);
        }

        public async Task<ICalculatedAmountInfo> GetCalculatedAmountInfoAsync(string settlementAssetId, string paymentAssetId, decimal amount, string merchantId)
        {
            var paymentInfo = await GetPaymentInfoAsync(settlementAssetId, paymentAssetId, amount, merchantId, new RequestMarkup());

            return new CalculatedAmountInfo
            {
                PaymentAmount = paymentInfo.PaymentAmount,
                ExchangeRate = paymentInfo.Rate
            };
        }

        public Task<IOrder> GetByLykkeOperationAsync(string operationId)
        {
            return _orderRepository.GetByLykkeOperationAsync(operationId);
        }
    }
}
