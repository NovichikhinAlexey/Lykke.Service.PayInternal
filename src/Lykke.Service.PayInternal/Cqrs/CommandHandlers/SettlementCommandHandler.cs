using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Cqrs.Commands;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Log;

namespace Lykke.Service.PayInternal.Cqrs.CommandHandlers
{
    [UsedImplicitly]
    public class SettlementCommandHandler
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ITransactionPublisher _transactionPublisher;
        private readonly ITransactionsService _transactionsService;
        private readonly int _transactionConfirmationCount;
        private readonly ILog _log;

        public SettlementCommandHandler(IPaymentRequestService paymentRequestService,
            ITransactionPublisher transactionPublisher, ITransactionsService transactionsService,
            int transactionConfirmationCount, ILogFactory logFactory)
        {
            _paymentRequestService = paymentRequestService;
            _transactionPublisher = transactionPublisher;
            _transactionsService = transactionsService;
            _transactionConfirmationCount = transactionConfirmationCount;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(SettlementInProgressCommand command, IEventPublisher publisher)
        {
            _log.Info("Settlement is in progress.", new { command.MerchantId, command.PaymentRequestId });

            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(
                command.MerchantId, command.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(command.MerchantId, command.PaymentRequestId);

            IPaymentRequestTransaction settlementTx = await _transactionsService.CreateTransactionAsync(
                new CreateTransactionCommand
                {
                    Amount = paymentRequest.Amount,
                    Blockchain = BlockchainType.None,
                    AssetId = paymentRequest.SettlementAssetId,
                    WalletAddress = paymentRequest.WalletAddress,
                    DueDate = paymentRequest.DueDate,
                    IdentityType = TransactionIdentityType.Specific,
                    Identity = GetSettlementTransactionIdenty(command.MerchantId, 
                        command.PaymentRequestId),
                    Confirmations = 0,
                    Type = TransactionType.Settlement
                });

            await _transactionPublisher.PublishAsync(settlementTx);

            await _paymentRequestService.UpdateStatusAsync(command.MerchantId, command.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.SettlementInProgress
                });
        }

        [UsedImplicitly]
        public async Task Handle(SettlementTransferringToMarketCommand command, IEventPublisher publisher)
        {
            _log.Info($"Settlement transaction is received: {command.ToJson()}.", new
            {
                command.MerchantId,
                command.PaymentRequestId,
                command.TransactionHash
            });
            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(
                command.MerchantId, command.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(command.MerchantId, command.PaymentRequestId);

            IPaymentRequestTransaction settlementTx = await _transactionsService.CreateTransactionAsync(
                new CreateTransactionCommand
                {
                    Amount = command.TransactionAmount,
                    Blockchain = BlockchainType.Bitcoin,
                    AssetId = command.TransactionAssetId,
                    WalletAddress = command.DestinationAddress,
                    DueDate = paymentRequest.DueDate,
                    IdentityType = TransactionIdentityType.Hash,
                    Identity = command.TransactionHash,
                    Confirmations = 0,
                    Type = TransactionType.Settlement
                });

            await _transactionPublisher.PublishAsync(settlementTx);
        }

        [UsedImplicitly]
        public async Task Handle(SettledCommand command, IEventPublisher publisher)
        {
            _log.Info("Settlement is completed.", new { command.MerchantId, command.PaymentRequestId });

            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(
                command.MerchantId, command.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(command.MerchantId, command.PaymentRequestId);

            await _transactionsService.UpdateAsync(new CompleteSettlementOutTxCommand()
            {
                Blockchain = BlockchainType.None,
                IdentityType = TransactionIdentityType.Specific,
                Identity = GetSettlementTransactionIdenty(command.MerchantId,
                    command.PaymentRequestId),
                WalletAddress = paymentRequest.WalletAddress,
                Confirmations = _transactionConfirmationCount
            });

            await _paymentRequestService.UpdateStatusAsync(command.MerchantId, command.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.Settled
                });
        }

        [UsedImplicitly]
        public Task Handle(SettlementErrorCommand command, IEventPublisher publisher)
        {
            _log.Info($"Settlement is failed: {command.ToJson()}.", new
            {
                command.MerchantId,
                command.PaymentRequestId
            });

            return _paymentRequestService.UpdateStatusAsync(command.MerchantId, command.PaymentRequestId,
                new PaymentRequestStatusInfo()
                {
                    Status = PaymentRequestStatus.SettlementError,
                    ProcessingError = command.Error
                });
        }

        private string GetSettlementTransactionIdenty(string merchantId, string paymentRequestId)
        {
            return $"Settlement_{merchantId}_{paymentRequestId}";
        }
    }
}
