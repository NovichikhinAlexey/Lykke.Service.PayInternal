using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IOrderService _orderService;
        private readonly IPaymentRequestPublisher _paymentRequestPublisher;
        private readonly ITransferService _transferService;
        private readonly IPaymentRequestStatusResolver _paymentRequestStatusResolver;
        private readonly IWalletManager _walletsManager;
        private readonly ITransactionsService _transactionsService;
        private readonly ILog _log;

        public PaymentRequestService(
            IPaymentRequestRepository paymentRequestRepository,
            IOrderService orderService,
            IPaymentRequestPublisher paymentRequestPublisher,
            ITransferService transferService,
            IPaymentRequestStatusResolver paymentRequestStatusResolver,
            ILog log, 
            IWalletManager walletsManager, 
            ITransactionsService transactionsService)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _orderService = orderService;
            _paymentRequestPublisher = paymentRequestPublisher;
            _transferService = transferService;
            _paymentRequestStatusResolver = paymentRequestStatusResolver;
            _log = log;
            _walletsManager = walletsManager;
            _transactionsService = transactionsService;
        }

        public async Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId);
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);
        }

        public async Task<PaymentRequestRefund> GetRefundInfoAsync(string walletAddress)
        {
            IReadOnlyList<IPaymentRequestTransaction> transactions =
                (await _transactionsService.GetByWalletAsync(walletAddress)).Where(x => x.IsRefund()).ToList();

            if (!transactions.Any()) 
                return null;

            IEnumerable<string> transferIds = transactions.Unique(x => x.TransferId).ToList();

            if (transferIds.MoreThanOne())
                throw new MultiTransactionRefundNotSupportedException();

            Transfer transfer = await _transferService.GetAsync(transferIds.Single());

            return new PaymentRequestRefund
            {
                Amount = transfer.Amounts.Sum(x => x.Amount),
                Timestamp = transfer.CreatedOn,
                Address = transfer.Amounts.Unique(x => x.Destination).Single(),
                DueDate = transactions.OrderByDescending(x => x.DueDate).First().DueDate,
                Transactions = Mapper.Map<IEnumerable<PaymentRequestRefundTransaction>>(transactions)
            };
        }

        public async Task<IPaymentRequest> FindAsync(string walletAddress)
        {
            return await _paymentRequestRepository.FindAsync(walletAddress);
        }

        public async Task<IPaymentRequest> CreateAsync(IPaymentRequest paymentRequest)
        {
            paymentRequest.Status = PaymentRequestStatus.New;

            paymentRequest.Timestamp = DateTime.UtcNow;

            IVirtualWallet wallet = await _walletsManager.CreateAsync(paymentRequest.MerchantId,
                paymentRequest.DueDate, paymentRequest.PaymentAssetId);

            paymentRequest.WalletAddress = wallet.Id;

            IPaymentRequest createdPaymentRequest = await _paymentRequestRepository.InsertAsync(paymentRequest);

            await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CreateAsync),
                paymentRequest.ToJson(), "Payment request created.");

            return createdPaymentRequest;
        }

        public async Task CancelAsync(string merchantId, string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);

            if (paymentRequest.Status != PaymentRequestStatus.New)
                throw new NotAllowedStatusException(paymentRequest.Status);

            await UpdateStatusAsync(paymentRequest.WalletAddress,
                new PaymentRequestStatusInfo {Status = PaymentRequestStatus.Cancelled});
        }

        public async Task<IPaymentRequest> CheckoutAsync(string merchantId, string paymentRequestId, bool force = false)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);

            // Don't create new order if payment reqest status not new. 
            if (paymentRequest.Status != PaymentRequestStatus.New)
                return paymentRequest;

            IOrder order = await _orderService.GetLatestOrCreateAsync(paymentRequest, force);

            if (paymentRequest.OrderId != order.Id)
            {
                paymentRequest.OrderId = order.Id;
                await _paymentRequestRepository.UpdateAsync(paymentRequest);

                await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CheckoutAsync),
                    paymentRequest.ToJson(), "Payment request order updated.");
            }

            return paymentRequest;
        }

        public async Task UpdateStatusAsync(string walletAddress, PaymentRequestStatusInfo statusInfo = null)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);

            PaymentRequestStatusInfo newStatusInfo =
                statusInfo ?? await _paymentRequestStatusResolver.GetStatus(walletAddress);

            paymentRequest.Status = newStatusInfo.Status;
            paymentRequest.PaidDate = newStatusInfo.Date;
            paymentRequest.PaidAmount = newStatusInfo.Amount;
            paymentRequest.ProcessingError = paymentRequest.Status == PaymentRequestStatus.Error
                ? newStatusInfo.ProcessingError
                : PaymentRequestProcessingError.None;

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            //todo: move to separate builder service
            PaymentRequestRefund refundInfo = await GetRefundInfoAsync(paymentRequest.WalletAddress);

            await _paymentRequestPublisher.PublishAsync(paymentRequest, refundInfo);
        }

        public async Task UpdateStatusByTransactionAsync(string transactionId, BlockchainType blockchain)
        {
            IPaymentRequestTransaction tx = await _transactionsService.GetByIdAsync(transactionId, blockchain);

            if (tx == null)
                throw new TransactionNotFoundException(transactionId, blockchain);

            await UpdateStatusAsync(tx.WalletAddress);
        }
    }
}
