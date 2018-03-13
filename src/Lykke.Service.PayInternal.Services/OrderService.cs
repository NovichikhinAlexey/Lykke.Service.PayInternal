using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
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
        private readonly ICalculationService _calculationService;
        private readonly ILog _log;
        private readonly TimeSpan _orderExpiration;
        private readonly int _transactionConfirmationCount;

        public OrderService(
            IOrderRepository orderRepository,
            IAssetsLocalCache assetsLocalCache,
            IMerchantRepository merchantRepository,
            ICalculationService calculationService,
            ILog log,
            TimeSpan orderExpiration,
            int transactionConfirmationCount)
        {
            _orderRepository = orderRepository;
            _assetsLocalCache = assetsLocalCache;
            _merchantRepository = merchantRepository;
            _calculationService = calculationService;
            _log = log;
            _orderExpiration = orderExpiration;
            _transactionConfirmationCount = transactionConfirmationCount;
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, string orderId)
        {
            return await _orderRepository.GetAsync(paymentRequestId, orderId);
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, DateTime date)
        {
            IReadOnlyList<IOrder> orders = await _orderRepository.GetAsync(paymentRequestId);

            return orders
                .Where(o => date < o.DueDate)
                .OrderBy(o => o.DueDate)
                .FirstOrDefault();
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
                await _assetsLocalCache.GetAssetPairAsync(paymentRequest.PaymentAssetId,
                    paymentRequest.SettlementAssetId);

            var merchantMarkup = new MerchantMarkup
            {
                LpPercent = merchant.LpMarkupPercent,
                DeltaSpread = merchant.DeltaSpread,
                LpPips = merchant.LpMarkupPips,
                LpFixedFee = merchant.MarkupFixedFee
            };

            var requestMarkup = new RequestMarkup
            {
                Percent = paymentRequest.MarkupPercent,
                Pips = paymentRequest.MarkupPips,
                FixedFee = paymentRequest.MarkupFixedFee
            };

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
                DueDate = DateTime.UtcNow.Add(_orderExpiration),
                CreatedDate = DateTime.UtcNow,
                ExchangeRate = rate
            };

            IOrder createdOrder = await _orderRepository.InsertAsync(order);

            await _log.WriteInfoAsync(nameof(OrderService), nameof(GetLatestOrCreateAsync), order.ToJson(),
                "Order created.");

            return createdOrder;
        }

        public async Task<PaymentRequestStatusInfo> GetPaymentRequestStatus(IReadOnlyList<IBlockchainTransaction> transactions,
            string paymentRequestId)
        {
            if (!transactions.Any())
                return PaymentRequestStatusInfo.New();

            decimal btcPaid;

            var assetId = transactions.GetAssetId();

            switch (assetId)
            {
                case "BTC":
                    btcPaid = transactions.GetTotal();
                    break;
                case "Satoshi":
                    btcPaid = transactions.GetTotal().SatoshiToBtc();
                    break;
                default:
                    throw new UnexpectedAssetException(assetId);
            }

            var paidDate = transactions.GetLatestDate();

            var actualOrder = await GetAsync(paymentRequestId, paidDate);

            if (actualOrder == null)
                return PaymentRequestStatusInfo.Error("EXPIRED", btcPaid, paidDate);

            bool allConfirmed = transactions.All(o => o.Confirmations >= _transactionConfirmationCount);
            

            if (!allConfirmed)
                return PaymentRequestStatusInfo.InProcess();

            decimal btcToBePaid = actualOrder.PaymentAmount;

            var fulfillment = await _calculationService.CalculateBtcAmountFullfillmentAsync(btcToBePaid, btcPaid);

            switch (fulfillment)
            {
                case AmountFullFillmentStatus.Below:
                    return PaymentRequestStatusInfo.Error("AMOUNT BELOW", btcPaid, paidDate);
                case AmountFullFillmentStatus.Exact:
                    return PaymentRequestStatusInfo.Confirmed(btcPaid, paidDate);
                case AmountFullFillmentStatus.Above:
                    return PaymentRequestStatusInfo.Error("AMOUNT ABOVE", btcPaid, paidDate);
                default: throw new Exception("Unexpected amount fullfillment status");
            }
        }
    }
}
