using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
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
        private readonly IPaymentRequestTransactionRepository _transactionRepository;
        private readonly IOrderService _orderService;
        private readonly ICalculationService _calculationService;

        public PaymentRequestStatusResolver(
            int transactionConfirmationCount,
            IPaymentRequestRepository paymentRequestRepository,
            IPaymentRequestTransactionRepository transactionRepository,
            IOrderService orderService,
            ICalculationService calculationService)
        {
            _transactionConfirmationCount = transactionConfirmationCount;
            _paymentRequestRepository = paymentRequestRepository;
            _transactionRepository = transactionRepository;
            _orderService = orderService;
            _calculationService = calculationService;
        }

        public async Task<PaymentRequestStatusInfo> GetStatus(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);
            
            IReadOnlyList<IPaymentRequestTransaction> txs =
                await _transactionRepository.GetAsync(paymentRequest.WalletAddress);

            PaymentRequestStatusInfo paymentStatusInfo;

            if (txs.Any(x => x.IsSettlement()))
            {
                paymentStatusInfo = await GetStatusForSettlement(paymentRequest);
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

        private async Task<PaymentRequestStatusInfo> GetStatusForSettlement(IPaymentRequest paymentRequest)
        {
            throw new TransactionTypeNotSupportedException();
        }

        private async Task<PaymentRequestStatusInfo> GetStatusForRefund(IPaymentRequest paymentRequest)
        {
            IReadOnlyList<IPaymentRequestTransaction> txs =
                (await _transactionRepository.GetAsync(paymentRequest.WalletAddress)).Where(x => x.IsRefund()).ToList();

            if (txs.All(x => x.Confirmed(_transactionConfirmationCount)))
                return PaymentRequestStatusInfo.Refunded();

            return txs.Any(x => !x.Confirmed(_transactionConfirmationCount) && x.Expired())
                ? PaymentRequestStatusInfo.Error(PaymentRequestErrorType.RefundNotConfirmed)
                : PaymentRequestStatusInfo.RefundInProgress();
        }

        private async Task<PaymentRequestStatusInfo> GetStatusForPayment(IPaymentRequest paymentRequest)
        {
            IReadOnlyList<IPaymentRequestTransaction> txs =
                (await _transactionRepository.GetAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment()).ToList();

            if (!txs.Any())
                return PaymentRequestStatusInfo.New();

            decimal btcPaid;

            var assetId = txs.GetAssetId();

            switch (assetId)
            {
                case LykkeConstants.BitcoinAssetId:
                    btcPaid = txs.GetTotal();
                    break;
                case LykkeConstants.SatoshiAssetId:
                    btcPaid = txs.GetTotal().SatoshiToBtc();
                    break;
                default:
                    throw new UnexpectedAssetException(assetId);
            }

            var paidDate = txs.GetLatestDate();

            var actualOrder = await _orderService.GetAsync(paymentRequest.Id, paidDate);

            if (actualOrder == null)
                return PaymentRequestStatusInfo.Error(PaymentRequestErrorType.PaymentExpired, btcPaid, paidDate);

            bool allConfirmed = txs.All(x => x.Confirmed(_transactionConfirmationCount));

            if (!allConfirmed)
                return PaymentRequestStatusInfo.InProcess();

            decimal btcToBePaid = actualOrder.PaymentAmount;

            var fulfillment = await _calculationService.CalculateBtcAmountFullfillmentAsync(btcToBePaid, btcPaid);

            switch (fulfillment)
            {
                case AmountFullFillmentStatus.Below:
                    return PaymentRequestStatusInfo.Error(PaymentRequestErrorType.PaymentAmountBelow, btcPaid, paidDate);
                case AmountFullFillmentStatus.Exact:
                    return PaymentRequestStatusInfo.Confirmed(btcPaid, paidDate);
                case AmountFullFillmentStatus.Above:
                    return PaymentRequestStatusInfo.Error(PaymentRequestErrorType.PaymentAmountAbove, btcPaid, paidDate);
                default: throw new Exception("Unexpected amount fullfillment status");
            }
        }
    }
}
