using System;
using System.Collections.Generic;
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
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Orders;
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
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICalculationService _calculationService;
        private readonly IMarkupService _markupService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;
        private readonly OrderExpirationPeriodsSettings _orderExpirationPeriods;

        public OrderService(
            [NotNull] IOrderRepository orderRepository,
            [NotNull] IMerchantRepository merchantRepository,
            [NotNull] ICalculationService calculationService,
            [NotNull] ILogFactory logFactory,
            [NotNull] OrderExpirationPeriodsSettings orderExpirationPeriods,
            [NotNull] IMarkupService markupService,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _orderRepository = orderRepository;
            _merchantRepository = merchantRepository;
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
            IMerchant merchant = await _merchantRepository.GetAsync(merchantId);

            if (merchant == null)
                throw new MerchantNotFoundException(merchantId);

            string lykkePaymentAssetId = await _lykkeAssetsResolver.GetLykkeId(paymentAssetId);

            string lykkeSettlementAssetId = await _lykkeAssetsResolver.GetLykkeId(settlementAssetId);

            string assetPairId = $"{paymentAssetId}{settlementAssetId}";

            IMarkup merchantMarkup;

            try
            {
                merchantMarkup = await _markupService.ResolveAsync(merchant.Id, assetPairId);
            }
            catch (MarkupNotFoundException e)
            {
                _log.Error(e, new
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
                _log.Error(e, new { e.PriceType });

                throw;
            }
            catch (UnexpectedAssetPairPriceMethodException e)
            {
                _log.Error(e, new { e.PriceMethod });

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
    }
}
