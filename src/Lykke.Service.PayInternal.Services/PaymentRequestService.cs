using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly int _transactionConfirmationCount;
        private readonly ITransferService _transferService;
        private readonly TimeSpan _refundExpirationPeriod;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly ITransactionsService _transactionsService;
        private readonly ILog _log;

        public PaymentRequestService(
            IPaymentRequestRepository paymentRequestRepository,
            IMerchantWalletsService merchantWalletsService,
            IPaymentRequestTransactionRepository transactionRepository,
            IOrderService orderService,
            IPaymentRequestPublisher paymentRequestPublisher,
            int transactionConfirmationCount,
            ITransferService transferService,
            TimeSpan refundExpirationPeriod,
            ITransactionPublisher transactionPublisher,
            ITransactionsService transactionsService,
            ILog log)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _merchantWalletsService = merchantWalletsService;
            _transactionRepository = transactionRepository;
            _orderService = orderService;
            _paymentRequestPublisher = paymentRequestPublisher;
            _transactionConfirmationCount = transactionConfirmationCount;
            _transferService = transferService;
            _refundExpirationPeriod = refundExpirationPeriod;
            _transactionPublisher = transactionPublisher;
            _transactionsService = transactionsService;
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

            if(paymentRequest == null)
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

        public async Task ProcessAsync(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);
            
            //todo: we can get transactions by partition, will be faster
            IReadOnlyList<IPaymentRequestTransaction> txs = await _transactionRepository.GetByPaymentRequest(paymentRequest.Id);

            IEnumerable<TransactionType> txTypes = txs.Select(x => x.TransactionType).Distinct().ToList();

            PaymentRequestStatusInfo paymentStatusInfo;
            // todo: rethink status calculation implementation, isolate
            if (txTypes.Contains(TransactionType.Settlement))
            {
                throw new TransactionTypeNotSupportedException(TransactionType.Settlement.ToString());
            } else if (txTypes.Contains(TransactionType.Refund))
            {
                IReadOnlyList<IPaymentRequestTransaction> refundTxs =
                    txs.Where(x => x.TransactionType == TransactionType.Refund).ToList();

                if (refundTxs.All(x => x.Confirmations >= _transactionConfirmationCount))
                {
                    paymentStatusInfo = PaymentRequestStatusInfo.Refunded();
                }
                else
                {
                    paymentStatusInfo =
                        refundTxs.Any(x =>
                            x.Confirmations < _transactionConfirmationCount && x.DueDate < DateTime.UtcNow)
                            ? PaymentRequestStatusInfo.Error("REFUND NOT CONFIRMED")
                            : PaymentRequestStatusInfo.RefundInProgress();
                }
            } else if (txTypes.Contains(TransactionType.Payment))
            {
                IReadOnlyList<IPaymentRequestTransaction> paymentTxs =
                    txs.Where(x => x.TransactionType == TransactionType.Payment).ToList();

                paymentStatusInfo = await _orderService.GetPaymentRequestStatus(paymentTxs, paymentRequest.Id);
            }
            else
                throw new Exception("Inconsistent paymentRequest status");

            paymentRequest.Status = paymentStatusInfo.Status;
            paymentRequest.PaidDate = paymentStatusInfo.Date;
            paymentRequest.PaidAmount = paymentStatusInfo.Amount;
            if (paymentRequest.Status == PaymentRequestStatus.Error)
            {
                paymentRequest.Error = paymentStatusInfo.Details;
            }

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            await _paymentRequestPublisher.PublishAsync(paymentRequest);
        }

        public async Task ProcessByTransactionAsync(string transactionId)
        {
            IEnumerable<IPaymentRequestTransaction> txs = await _transactionRepository.GetByTransactionAsync(transactionId);

            IEnumerable<string> walletAddresses = txs.Select(x => x.WalletAddress).Distinct();

            foreach (string walletAddress in walletAddresses)
            {
                await ProcessAsync(walletAddress);
            }
        }

        public async Task<RefundResult> RefundAsync(RefundCommand command)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(command.MerchantId, command.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(command.MerchantId, command.PaymentRequestId);

            if (!paymentRequest.StatusValidForRefund())
                throw new NotAllowedStatusException(paymentRequest.Status);

            IEnumerable<IPaymentRequestTransaction> paymentTxs =
                (await _transactionRepository.GetAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment()).ToList();

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

            DateTime refundDueDate = DateTime.UtcNow.Add(_refundExpirationPeriod);

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
                        DueDate = refundDueDate
                    });

                //todo: think of moving this call inside  _transactionsService
                await _transactionPublisher.PublishAsync(refundTransaction);
            }

            return await PrepareRefundResult(paymentRequest, transferResult, refundDueDate);
        }

        private async Task<RefundResult> PrepareRefundResult(IPaymentRequest paymentRequest, TransferResult transferResult, DateTime refundDueDate)
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
                DueDate = refundDueDate
            };
        }
    }
}
