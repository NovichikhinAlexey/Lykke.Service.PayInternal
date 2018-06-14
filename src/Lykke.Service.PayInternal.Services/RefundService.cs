using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class RefundService : IRefundService
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ITransactionsService _transactionsService;
        private readonly ITransferService _transferService;
        private readonly TimeSpan _refundExpirationPeriod;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly IBlockchainAddressValidator _blockchainAddressValidator;
        private readonly ILog _log;

        public RefundService(
            IPaymentRequestService paymentRequestService, 
            ITransactionsService transactionsService,
            ITransferService transferService,
            TimeSpan refundExpirationPeriod,
            ITransactionPublisher transactionPublisher,
            ILog log, 
            IBlockchainAddressValidator blockchainAddressValidator)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _refundExpirationPeriod = refundExpirationPeriod;
            _transactionPublisher =
                transactionPublisher ?? throw new ArgumentNullException(nameof(transactionPublisher));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _blockchainAddressValidator = blockchainAddressValidator ??
                                          throw new ArgumentNullException(nameof(blockchainAddressValidator));
        }

        public async Task<RefundResult> ExecuteAsync(string merchantId, string paymentRequestId,
            string destinationWalletAddress)
        {
            IPaymentRequest paymentRequest =
                await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new RefundValidationException(RefundErrorType.PaymentRequestNotFound);

            if (!paymentRequest.StatusValidForRefund())
                throw new RefundValidationException(RefundErrorType.NotAllowedInStatus);

            IEnumerable<IPaymentRequestTransaction> paymentTxs =
                (await _transactionsService.GetByWalletAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment()).ToList();

            if (!paymentTxs.Any())
                throw new RefundValidationException(RefundErrorType.NoPaymentTransactions);

            if (paymentTxs.MoreThanOne())
                throw new RefundValidationException(RefundErrorType.MultitransactionNotSupported);

            IPaymentRequestTransaction tx = paymentTxs.Single();

            bool isValidAddress = string.IsNullOrWhiteSpace(destinationWalletAddress) ||
                                  await _blockchainAddressValidator.Execute(destinationWalletAddress, tx.Blockchain);
            if (!isValidAddress)
                throw new RefundValidationException(RefundErrorType.InvalidDestinationAddress);

            if (!tx.SourceWalletAddresses.Any())
                throw new RefundValidationException(RefundErrorType.InvalidDestinationAddress);

            if (string.IsNullOrWhiteSpace(destinationWalletAddress))
            {
                if (tx.SourceWalletAddresses.MoreThanOne())
                    throw new RefundValidationException(RefundErrorType.InvalidDestinationAddress);
            }

            //validation finished, refund request accepted
            await _paymentRequestService.UpdateStatusAsync(paymentRequest.WalletAddress,
                PaymentRequestStatusInfo.RefundInProgress());

            TransferResult transferResult;

            DateTime refundDueDate;

            try
            {
                TransferCommand refundTransferCommand = Mapper.Map<TransferCommand>(tx,
                    opts => opts.Items["destinationAddress"] = destinationWalletAddress);

                transferResult = await _transferService.ExecuteAsync(refundTransferCommand);

                refundDueDate = transferResult.Timestamp.Add(_refundExpirationPeriod);

                foreach (var transferResultTransaction in transferResult.Transactions)
                {
                    if (!string.IsNullOrEmpty(transferResultTransaction.Error))
                    {
                        await _log.WriteWarningAsync(nameof(RefundService), nameof(ExecuteAsync),
                            transferResultTransaction.ToJson(), "Transaction failed");

                        continue;
                    }

                    IPaymentRequestTransaction refundTransaction = await _transactionsService.CreateTransactionAsync(
                        new CreateTransactionCommand
                        {
                            Amount = transferResultTransaction.Amount,
                            AssetId = transferResultTransaction.AssetId,
                            Confirmations = 0,
                            Hash = transferResultTransaction.Hash,
                            WalletAddress = paymentRequest.WalletAddress,
                            Type = TransactionType.Refund,
                            Blockchain = transferResult.Blockchain,
                            FirstSeen = null,
                            DueDate = refundDueDate,
                            TransferId = transferResult.Id,
                            IdentityType = transferResultTransaction.IdentityType,
                            Identity = transferResultTransaction.Identity
                        });

                    await _transactionPublisher.PublishAsync(refundTransaction);
                }

                if (!transferResult.HasSuccess())
                    throw new RefundOperationFailedException {TransferErrors = transferResult.GetErrors()};

                if (transferResult.HasError())
                    throw new RefundOperationPartiallyFailedException {TransferErrors = transferResult.GetErrors()};
            }
            catch (Exception)
            {
                await _paymentRequestService.UpdateStatusAsync(paymentRequest.WalletAddress,
                    PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.UnknownRefund));

                throw;
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
                Amount = transferResult.GetSuccedeedTxs().Sum(x => x.Amount),
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
