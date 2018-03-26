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
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly IPaymentRequestTransactionRepository _transactionRepository;
        private readonly IOrderService _orderService;
        private readonly IPaymentRequestPublisher _paymentRequestPublisher;
        private readonly ITransferService _transferService;
        private readonly TimeSpan _refundExpirationPeriod;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly ITransactionsService _transactionsService;
        private readonly IPaymentRequestStatusResolver _paymentRequestStatusResolver;
        private readonly ILog _log;

        public PaymentRequestService(
            IPaymentRequestRepository paymentRequestRepository,
            IMerchantWalletsService merchantWalletsService,
            IPaymentRequestTransactionRepository transactionRepository,
            IOrderService orderService,
            IPaymentRequestPublisher paymentRequestPublisher,
            ITransferService transferService,
            TimeSpan refundExpirationPeriod,
            ITransactionPublisher transactionPublisher,
            ITransactionsService transactionsService,
            IPaymentRequestStatusResolver paymentRequestStatusResolver,
            ILog log)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _merchantWalletsService = merchantWalletsService;
            _transactionRepository = transactionRepository;
            _orderService = orderService;
            _paymentRequestPublisher = paymentRequestPublisher;
            _transferService = transferService;
            _refundExpirationPeriod = refundExpirationPeriod;
            _transactionPublisher = transactionPublisher;
            _transactionsService = transactionsService;
            _paymentRequestStatusResolver = paymentRequestStatusResolver;
            _log = log;
        }

        public async Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId);
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);
        }

        public async Task<PaymentRequestRefund> GetRefundInfoAsync(string paymentRequestId)
        {
            //todo: move to transactionsService
            IReadOnlyList<IPaymentRequestTransaction> transactions =
                (await _transactionRepository.GetByPaymentRequest(paymentRequestId)).Where(x => x.IsRefund()).ToList();

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
            paymentRequest.WalletAddress =
                await _merchantWalletsService.CreateAddress(new CreateWallet
                {
                    DueDate = paymentRequest.DueDate,
                    MerchantId = paymentRequest.MerchantId
                });

            IPaymentRequest createdPaymentRequest = await _paymentRequestRepository.InsertAsync(paymentRequest);

            await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CreateAsync),
                paymentRequest.ToJson(), "Payment request created.");

            return createdPaymentRequest;
        }

        public async Task<IPaymentRequest> CheckoutAsync(string merchantId, string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);

            // Don't create new order if payment reqest status not new. 
            if (paymentRequest.Status != PaymentRequestStatus.New)
                return paymentRequest;

            IOrder order = await _orderService.GetLatestOrCreateAsync(paymentRequest);

            if (paymentRequest.OrderId != order.Id)
            {
                paymentRequest.OrderId = order.Id;
                await _paymentRequestRepository.UpdateAsync(paymentRequest);

                await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CheckoutAsync),
                    paymentRequest.ToJson(), "Payment request order updated.");
            }

            return paymentRequest;
        }

        public async Task UpdateStatusAsync(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);

            PaymentRequestStatusInfo newStatusInfo = await _paymentRequestStatusResolver.GetStatus(walletAddress);

            paymentRequest.Status = newStatusInfo.Status;
            paymentRequest.PaidDate = newStatusInfo.Date;
            paymentRequest.PaidAmount = newStatusInfo.Amount;
            if (paymentRequest.Status == PaymentRequestStatus.Error)
            {
                paymentRequest.Error = newStatusInfo.Details;
            }

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            await _paymentRequestPublisher.PublishAsync(paymentRequest);
        }

        public async Task UpdateStatusByTransactionAsync(string transactionId)
        {
            IEnumerable<IPaymentRequestTransaction> txs =
                await _transactionRepository.GetByTransactionAsync(transactionId);
            
            foreach (string walletAddress in txs.Unique(x => x.WalletAddress))
            {
                await UpdateStatusAsync(walletAddress);
            }
        }

        public async Task<RefundResult> RefundAsync(RefundCommand command)
        {
            IPaymentRequest paymentRequest =
                await _paymentRequestRepository.GetAsync(command.MerchantId, command.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(command.MerchantId, command.PaymentRequestId);

            if (!paymentRequest.StatusValidForRefund())
                throw new NotAllowedStatusException(paymentRequest.Status);

            IEnumerable<IPaymentRequestTransaction> paymentTxs =
                (await _transactionRepository.GetAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment())
                .ToList();

            if (!paymentTxs.Any())
                throw new NoTransactionsToRefundException(paymentRequest.Id);

            if (paymentTxs.MoreThanOne())
                throw new MultiTransactionRefundNotSupportedException(paymentTxs.Count());

            IPaymentRequestTransaction tx = paymentTxs.Single();

            if (!tx.SourceWalletAddresses.Any())
                throw new NoTransactionsToRefundException(paymentRequest.Id);

            if (string.IsNullOrWhiteSpace(command.DestinationAddress))
            {
                if (tx.SourceWalletAddresses.MoreThanOne())
                    throw new MultiTransactionRefundNotSupportedException(tx.SourceWalletAddresses.Length);
            }

            TransferResult transferResult =
                await _transferService.ExecuteAsync(tx.ToRefundTransferCommand(command.DestinationAddress));

            DateTime refundDueDate = transferResult.Timestamp.Add(_refundExpirationPeriod);

            foreach (var transferResultTransaction in transferResult.Transactions)
            {
                if (!string.IsNullOrEmpty(transferResultTransaction.Error))
                {
                    await _log.WriteWarningAsync(nameof(PaymentRequestService), nameof(RefundAsync),
                        transferResultTransaction.ToJson(), "Transaction failed");

                    continue;
                }

                IPaymentRequestTransaction refundTransaction = await _transactionsService.CreateTransaction(
                    new CreateTransaction
                    {
                        Amount = transferResultTransaction.Amount,
                        AssetId = transferResultTransaction.AssetId,
                        Confirmations = 0,
                        TransactionId = transferResultTransaction.Hash,
                        WalletAddress = paymentRequest.WalletAddress,
                        Type = TransactionType.Refund,
                        Blockchain = transferResult.Blockchain,
                        FirstSeen = null,
                        DueDate = refundDueDate,
                        TransferId = transferResult.Id
                    });

                //todo: think of moving this call inside  _transactionsService
                await _transactionPublisher.PublishAsync(refundTransaction);
            }

            return await PrepareRefundResult(paymentRequest, transferResult, refundDueDate);
        }

        private async Task<RefundResult> PrepareRefundResult(IPaymentRequest paymentRequest,
            TransferResult transferResult, DateTime refundDueDate)
        {
            var assetIds = transferResult.Transactions.Unique(x => x.AssetId).ToList();

            if (assetIds.MoreThanOne())
                await _log.WriteWarningAsync(nameof(PaymentRequestService), nameof(PrepareRefundResult), new
                {
                    PaymentResuest = paymentRequest,
                    RefundTransferResult = transferResult
                }.ToJson(), "Multiple assets are not expected");

            return new RefundResult
            {
                Amount = transferResult.Transactions
                    .Where(x => string.IsNullOrEmpty(x.Error))
                    .Sum(x => x.Amount),
                AssetId = assetIds.Single(),
                PaymentRequestId = paymentRequest.Id,
                PaymentRequestWalletAddress = paymentRequest.WalletAddress,
                Transactions = transferResult.Transactions.Select(x => new RefundTransactionResult
                {
                    Amount = x.Amount,
                    AssetId = x.AssetId,
                    Blockchain = transferResult.Blockchain,
                    Hash = x.Hash,
                    SourceAddress = string.Join(";", x.Sources),
                    DestinationAddress = string.Join(";", x.Destinations)
                }),
                DueDate = refundDueDate,
                Timestamp = transferResult.Timestamp
            };
        }
    }
}
