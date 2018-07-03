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
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;
using Polly;

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
        private readonly ExpirationPeriodsSettings _expirationPeriods;
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly IDistributedLocksService _paymentLocksService;
        private readonly IDistributedLocksService _checkoutLocksService;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly ILog _log;

        public PaymentRequestService(
            [NotNull] IPaymentRequestRepository paymentRequestRepository,
            [NotNull] IOrderService orderService,
            [NotNull] IPaymentRequestPublisher paymentRequestPublisher,
            [NotNull] ITransferService transferService,
            [NotNull] IPaymentRequestStatusResolver paymentRequestStatusResolver,
            [NotNull] ILog log,
            [NotNull] IWalletManager walletsManager,
            [NotNull] ITransactionsService transactionsService,
            [NotNull] ExpirationPeriodsSettings expirationPeriods,
            [NotNull] IMerchantWalletService merchantWalletService, 
            [NotNull] IDistributedLocksService paymentLocksService, 
            [NotNull] ITransactionPublisher transactionPublisher, 
            [NotNull] IDistributedLocksService checkoutLocksService, 
            [NotNull] IAssetSettingsService assetSettingsService, 
            [NotNull] IBlockchainClientProvider blockchainClientProvider)
        {
            _paymentRequestRepository = paymentRequestRepository ?? throw new ArgumentNullException(nameof(paymentRequestRepository));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _paymentRequestPublisher = paymentRequestPublisher ?? throw new ArgumentNullException(nameof(paymentRequestPublisher));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _paymentRequestStatusResolver = paymentRequestStatusResolver ?? throw new ArgumentNullException(nameof(paymentRequestStatusResolver));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _walletsManager = walletsManager ?? throw new ArgumentNullException(nameof(walletsManager));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _expirationPeriods = expirationPeriods ?? throw new ArgumentNullException(nameof(expirationPeriods));
            _merchantWalletService = merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _paymentLocksService = paymentLocksService ?? throw new ArgumentNullException(nameof(paymentLocksService));
            _transactionPublisher = transactionPublisher ?? throw new ArgumentNullException(nameof(transactionPublisher));
            _checkoutLocksService = checkoutLocksService ?? throw new ArgumentNullException(nameof(checkoutLocksService));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
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

            IPaymentRequest paymentRequest = await FindAsync(walletAddress);

            return new PaymentRequestRefund
            {
                Amount = transfer.Amounts.Sum(x => x.Amount ?? 0),
                Timestamp = transfer.CreatedOn,
                Address = transfer.Amounts.Unique(x => x.Destination).Single(),
                DueDate = transactions.OrderByDescending(x => x.DueDate).First().DueDate ?? paymentRequest.DueDate,
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

            DateTime walletDueDate = paymentRequest.DueDate.Add(_expirationPeriods.WalletExtra);

            IVirtualWallet wallet = await _walletsManager.CreateAsync(paymentRequest.MerchantId, walletDueDate);

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

        public async Task<IPaymentRequest> CheckoutAsync(string merchantId, string paymentRequestId, bool force)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);

            // Don't create new order if payment reqest status not new. 
            if (paymentRequest.Status != PaymentRequestStatus.New)
                return paymentRequest;

            return await Policy
                .Handle<DistributedLockAcquireException>()
                .WaitAndRetryForeverAsync(
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.WriteErrorAsync("Acquiring checkout lock with retry", new {paymentRequestId}.ToJson(), ex))
                .ExecuteAsync(() => TryCheckoutAsync(paymentRequest, force));
        }

        public async Task UpdateStatusAsync(string walletAddress, PaymentRequestStatusInfo statusInfo = null)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);

            PaymentRequestStatusInfo newStatusInfo =
                statusInfo ?? await _paymentRequestStatusResolver.GetStatus(walletAddress);

            PaymentRequestStatus previousStatus = paymentRequest.Status;

            paymentRequest.Status = newStatusInfo.Status;
            paymentRequest.PaidDate = newStatusInfo.Date;
            paymentRequest.PaidAmount = newStatusInfo.Amount;
            paymentRequest.ProcessingError = paymentRequest.Status == PaymentRequestStatus.Error
                ? newStatusInfo.ProcessingError
                : PaymentRequestProcessingError.None;

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            // if we are updating status from "InProcess" to any other - we have to release the lock
            if (previousStatus == PaymentRequestStatus.InProcess)
                await _paymentLocksService.ReleaseLockAsync(paymentRequest.Id, paymentRequest.MerchantId);

            PaymentRequestRefund refundInfo = await GetRefundInfoAsync(paymentRequest.WalletAddress);

            await _paymentRequestPublisher.PublishAsync(paymentRequest, refundInfo);

            // doing settlement only once
            // Some flows assume we can get updates from blockchain multiple times for the same transaction
            // which leads to the same payment request status 
            if (paymentRequest.StatusValidForSettlement() && paymentRequest.Status != previousStatus)
                await SettleAsync(paymentRequest.MerchantId, paymentRequest.Id);
        }

        public async Task HandleExpiredAsync()
        {
            DateTime dateTo = DateTime.UtcNow;

            DateTime dateFrom = dateTo.Add(-_expirationPeriods.WalletExtra);

            IReadOnlyList<IPaymentRequest> expired = await _paymentRequestRepository.GetByDueDate(dateFrom, dateTo);

            IEnumerable<IPaymentRequest> eligibleForTransition =
                expired.Where(x => x.StatusValidForPastDueTransition()).ToList();

            if (eligibleForTransition.Any())
            {
                await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(HandleExpiredAsync),
                    $"Found payment requests eligible to move to Past Due: {eligibleForTransition.Count()}");
            }

            foreach (IPaymentRequest paymentRequest in eligibleForTransition)
            {
                await UpdateStatusAsync(paymentRequest.WalletAddress, PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.PaymentExpired));

                await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(HandleExpiredAsync),
                    $"Payment request with id {paymentRequest.Id} was moved to Past Due");
            }
        }

        public async Task<PaymentResult> PrePayAsync(PaymentCommand cmd)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(cmd.MerchantId, cmd.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(cmd.MerchantId, cmd.PaymentRequestId);

            string payerWalletAddress = (await _merchantWalletService.GetDefaultAsync(
                cmd.PayerMerchantId,
                paymentRequest.PaymentAssetId,
                PaymentDirection.Outgoing)).WalletAddress;

            await ValidateTransferBalance(payerWalletAddress, paymentRequest.PaymentAssetId, cmd.Amount);

            return new PaymentResult
            {
                Amount = cmd.Amount,
                AssetId = paymentRequest.PaymentAssetId,
                PaymentRequestId = paymentRequest.Id,
                PaymentRequestWalletAddress = paymentRequest.WalletAddress
            };
        }

        public async Task<PaymentResult> PayAsync(PaymentCommand cmd)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(cmd.MerchantId, cmd.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(cmd.MerchantId, cmd.PaymentRequestId);

            string payerWalletAddress = (await _merchantWalletService.GetDefaultAsync(
                cmd.PayerMerchantId,
                paymentRequest.PaymentAssetId, 
                PaymentDirection.Outgoing)).WalletAddress;

            string destinationWalletAddress = await _walletsManager.ResolveBlockchainAddressAsync(
                paymentRequest.WalletAddress,
                paymentRequest.PaymentAssetId);

            bool locked = await _paymentLocksService.TryAcquireLockAsync(
                paymentRequest.Id,
                cmd.MerchantId,
                paymentRequest.DueDate);

            if (!locked)
                throw new DistributedLockAcquireException(paymentRequest.Id);

            TransferResult transferResult;

            try
            {
                await UpdateStatusAsync(paymentRequest.WalletAddress, PaymentRequestStatusInfo.InProcess());

                transferResult = await _transferService.PayThrowFail(
                    paymentRequest.PaymentAssetId,
                    payerWalletAddress,
                    destinationWalletAddress,
                    cmd.Amount);

                foreach (var transferResultTransaction in transferResult.Transactions)
                {
                    IPaymentRequestTransaction paymentTx = await _transactionsService.CreateTransactionAsync(
                        new CreateTransactionCommand
                        {
                            Amount = transferResultTransaction.Amount,
                            Blockchain = transferResult.Blockchain,
                            AssetId = transferResultTransaction.AssetId,
                            WalletAddress = paymentRequest.WalletAddress,
                            DueDate = paymentRequest.DueDate,
                            IdentityType = transferResultTransaction.IdentityType,
                            Identity = transferResultTransaction.Identity,
                            Confirmations = 0,
                            Hash = transferResultTransaction.Hash,
                            TransferId = transferResult.Id,
                            Type = TransactionType.Payment,
                            SourceWalletAddresses = transferResultTransaction.Sources.ToArray()
                        });

                    await _transactionPublisher.PublishAsync(paymentTx);
                }
            }
            catch (Exception e)
            {
                PaymentRequestStatusInfo newStatus = e is InsufficientFundsException
                    ? PaymentRequestStatusInfo.New()
                    : PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.UnknownPayment);

                await UpdateStatusAsync(paymentRequest.WalletAddress, newStatus);

                await _paymentLocksService.ReleaseLockAsync(paymentRequest.Id, cmd.MerchantId);

                throw;
            }

            return new PaymentResult
            {
                PaymentRequestId = paymentRequest.Id,
                PaymentRequestWalletAddress = paymentRequest.WalletAddress,
                AssetId = transferResult.Transactions.Unique(x => x.AssetId).Single(),
                Amount = transferResult.GetSuccedeedTxs().Sum(x => x.Amount)
            };
        }

        public async Task<SettlementResult> SettleAsync(string merchantId, string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);

            if (!paymentRequest.StatusValidForSettlement())
                throw new SettlementValidationException("Invalid status");

            string sourceWalletAddress = await _walletsManager.ResolveBlockchainAddressAsync(
                paymentRequest.WalletAddress,
                paymentRequest.PaymentAssetId);

            string destWalletAddress = (await _merchantWalletService.GetDefaultAsync(
                paymentRequest.MerchantId,
                paymentRequest.PaymentAssetId,
                PaymentDirection.Incoming)).WalletAddress;

            TransferResult transferResult = await _transferService.SettleThrowFail(
                paymentRequest.PaymentAssetId,
                sourceWalletAddress,
                destWalletAddress);

            foreach (var transferResultTransaction in transferResult.Transactions)
            {
                IPaymentRequestTransaction settlementTx = await _transactionsService.CreateTransactionAsync(
                    new CreateTransactionCommand
                    {
                        Amount = transferResultTransaction.Amount,
                        Blockchain = transferResult.Blockchain,
                        AssetId = transferResultTransaction.AssetId,
                        WalletAddress = paymentRequest.WalletAddress,
                        DueDate = paymentRequest.DueDate,
                        IdentityType = transferResultTransaction.IdentityType,
                        Identity = transferResultTransaction.Identity,
                        Confirmations = 0,
                        Hash = transferResultTransaction.Hash,
                        TransferId = transferResult.Id,
                        Type = TransactionType.Settlement,
                        SourceWalletAddresses = transferResultTransaction.Sources.ToArray()
                    });

                await _transactionPublisher.PublishAsync(settlementTx);
            }

            return new SettlementResult
            {
                Blockchain = transferResult.Blockchain,
                WalletAddress = destWalletAddress,
                AssetId = paymentRequest.PaymentAssetId,
                Amount = transferResult.GetSuccedeedTxs().Sum(x => x.Amount)
            };
        }

        private async Task<IPaymentRequest> TryCheckoutAsync(IPaymentRequest paymentRequest, bool force)
        {
            bool locked = await _checkoutLocksService.TryAcquireLockAsync(
                paymentRequest.Id,
                paymentRequest.MerchantId,
                paymentRequest.DueDate);

            if (!locked)
                throw new DistributedLockAcquireException(paymentRequest.Id);

            try
            {
                await _walletsManager.EnsureBcnAddressAllocated(paymentRequest.MerchantId,
                    paymentRequest.WalletAddress,
                    paymentRequest.PaymentAssetId);

                IOrder order = await _orderService.GetLatestOrCreateAsync(paymentRequest, force);

                if (paymentRequest.OrderId != order.Id)
                {
                    paymentRequest.OrderId = order.Id;
                    await _paymentRequestRepository.UpdateAsync(paymentRequest);
                }
            }
            finally
            {
                await _checkoutLocksService.ReleaseLockAsync(paymentRequest.Id, paymentRequest.MerchantId);
            }

            return paymentRequest;
        }

        [AssertionMethod]
        private async Task ValidateTransferBalance(string walletAddress, string assetId, decimal transferAmount)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(assetId);

            IBlockchainApiClient blockchainApiClient = _blockchainClientProvider.Get(network);

            decimal balance = await blockchainApiClient.GetBalanceAsync(walletAddress, assetId);

            if (balance < transferAmount)
                throw new InsufficientFundsException(walletAddress, assetId);
        }
    }
}
