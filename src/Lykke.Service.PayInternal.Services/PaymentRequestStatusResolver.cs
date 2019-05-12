using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class PaymentRequestStatusResolver : IPaymentRequestStatusResolver
    {
        private readonly int _transactionConfirmationCount;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ITransactionsService _transactionsService;
        private readonly IOrderService _orderService;
        private readonly ICalculationService _calculationService;

        public PaymentRequestStatusResolver(
            int transactionConfirmationCount,
            IPaymentRequestRepository paymentRequestRepository,
            IOrderService orderService,
            ICalculationService calculationService, 
            ITransactionsService transactionsService)
        {
            _transactionConfirmationCount = transactionConfirmationCount;
            _paymentRequestRepository = paymentRequestRepository;
            _orderService = orderService;
            _calculationService = calculationService;
            _transactionsService = transactionsService;
        }

        public async Task<PaymentRequestStatusInfo> GetStatus(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);

            IReadOnlyList<IPaymentRequestTransaction> txs =
                await _transactionsService.GetByWalletAsync(paymentRequest.WalletAddress);

            PaymentRequestStatusInfo paymentStatusInfo;

            if (txs.Any(x => x.IsSettlement()))
            {
                //todo: better to have separate status for settlement
                paymentStatusInfo = await GetStatusForPayment(paymentRequest);
            } else if (txs.Any(x => x.IsRefund()))
            {
                paymentStatusInfo = await GetStatusForRefund(paymentRequest);
            } else if (txs.Any(x => x.IsPayment()))
            {
                paymentStatusInfo = await GetStatusForPayment(paymentRequest);
            }
            else
                throw new Exception("Inconsistent paymentRequest status");

            return paymentStatusInfo;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private Task<PaymentRequestStatusInfo> GetStatusForSettlement(IPaymentRequest paymentRequest)
        {
            throw new TransactionTypeNotSupportedException();
        }

        private async Task<PaymentRequestStatusInfo> GetStatusForRefund(IPaymentRequest paymentRequest)
        {
            IReadOnlyList<IPaymentRequestTransaction> txs =
                (await _transactionsService.GetByWalletAsync(paymentRequest.WalletAddress)).Where(x => x.IsRefund()).ToList();

            if (txs.All(x => x.Confirmed(_transactionConfirmationCount)))
                return PaymentRequestStatusInfo.Refunded();

            return txs.Any(x => !x.Confirmed(_transactionConfirmationCount) && x.Expired())
                ? PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.RefundNotConfirmed)
                : PaymentRequestStatusInfo.RefundInProgress();
        }

        private async Task<PaymentRequestStatusInfo> GetStatusForPayment(IPaymentRequest paymentRequest)
        {
            IReadOnlyList<IPaymentRequestTransaction> txs =
                (await _transactionsService.GetByWalletAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment()).ToList();

            if (!txs.Any())
                return (paymentRequest.DueDate < DateTime.UtcNow)
                    ? PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.PaymentExpired)
                    : PaymentRequestStatusInfo.New();

            decimal btcPaid;

            var assetId = txs.GetAssetId();

            switch (assetId)
            {
                case LykkeConstants.SatoshiAsset:
                    btcPaid = txs.GetTotal().SatoshiToBtc();
                    break;
                default:
                    btcPaid = txs.GetTotal();
                    break;
            }

            bool allConfirmed = txs.All(x => x.Confirmed(_transactionConfirmationCount));

            var paidDate = txs.GetLatestDate();

            if (paidDate > paymentRequest.DueDate)
            {
                if (allConfirmed)
                    return PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.LatePaid, btcPaid, paidDate);

                return paymentRequest.GetCurrentStatusInfo();
            }

            IOrder actualOrder = await _orderService.GetActualAsync(paymentRequest.Id, paidDate, btcPaid) ??
                                 await _orderService.GetLatestOrCreateAsync(paymentRequest);

            if (!allConfirmed)
                return PaymentRequestStatusInfo.InProcess();

            decimal btcToBePaid = actualOrder.PaymentAmount;

            var fulfillment = await _calculationService.CalculateBtcAmountFullfillmentAsync(btcToBePaid, btcPaid);

            switch (fulfillment)
            {
                case AmountFullFillmentStatus.Below:
                    return PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.PaymentAmountBelow, btcPaid, paidDate);
                case AmountFullFillmentStatus.Exact:
                    return PaymentRequestStatusInfo.Confirmed(btcPaid, paidDate);
                case AmountFullFillmentStatus.Above:
                    return PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.PaymentAmountAbove, btcPaid, paidDate);
                default: throw new Exception("Unexpected amount fullfillment status");
            }
        }
    }
}
