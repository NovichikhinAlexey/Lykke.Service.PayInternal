using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayHistory.AutorestClient.Models;
using Lykke.Service.PayHistory.Client;
using Lykke.Service.PayHistory.Client.Models;
using Lykke.Service.PayHistory.Client.Publisher;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Polly;
using Polly.Retry;

namespace Lykke.Service.PayInternal.Services
{
    public class WalletHistoryService : IWalletHistoryService
    {
        private readonly HistoryOperationPublisher _historyOperationPublisher;
        private readonly IPayHistoryClient _payHistoryClient;
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly RetryPolicy _publisherRetryPolicy;
        private readonly RetryPolicy _clientRetryPolicy;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly RetryPolicySettings _retryPolicySettings;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ILog _log;

        public WalletHistoryService(
            [NotNull] HistoryOperationPublisher historyOperationPublisher, 
            [NotNull] IMerchantWalletService merchantWalletService, 
            [NotNull] ILogFactory logFactory, 
            [NotNull] RetryPolicySettings retryPolicySettings, 
            [NotNull] IPayHistoryClient payHistoryClient)
        {
            _historyOperationPublisher = historyOperationPublisher ?? throw new ArgumentNullException(nameof(historyOperationPublisher));
            _merchantWalletService = merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _retryPolicySettings = retryPolicySettings ?? throw new ArgumentNullException(nameof(retryPolicySettings));
            _payHistoryClient = payHistoryClient ?? throw new ArgumentNullException(nameof(payHistoryClient));
            _log = logFactory.CreateLog(this);

            _publisherRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.WriteError("Publish wallet history with retry", null, ex));

            _clientRetryPolicy = Policy
                .Handle<Exception>(ex => !(ex is PayHistoryApiException))
                .WaitAndRetryAsync(
                    _retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.WriteError("Connecting to history service with retry", null, ex));
        }

        public async Task<string> PublishCashInAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            var operation = new HistoryOperation
            {
                Amount = cmd.Amount,
                AssetId = cmd.AssetId,
                Type = HistoryOperationType.Recharge,
                CreatedOn = DateTime.UtcNow,
                TxHash = cmd.TransactionHash,
                MerchantId = merchantWallet.MerchantId
            };

            await _publisherRetryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(operation));

            return operation.Id;
        }

        public async Task<string> PublishOutgoingExchangeAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            var operation = new HistoryOperation
            {
                Amount = cmd.Amount,
                AssetId = cmd.AssetId,
                Type = HistoryOperationType.OutgoingExchange,
                CreatedOn = DateTime.UtcNow,
                TxHash = cmd.TransactionHash,
                MerchantId = merchantWallet.MerchantId
            };

            await _publisherRetryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(operation));

            return operation.Id;
        }

        public async Task<string> PublishIncomingExchangeAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            var operation = new HistoryOperation
            {
                Amount = cmd.Amount,
                AssetId = cmd.AssetId,
                Type = HistoryOperationType.IncomingExchange,
                CreatedOn = DateTime.UtcNow,
                TxHash = cmd.TransactionHash,
                MerchantId = merchantWallet.MerchantId
            };

            await _publisherRetryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(operation));

            return operation.Id;
        }

        public async Task<string> PublishCashoutAsync(IWalletHistoryCashoutCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            var operation = new HistoryOperation
            {
                Amount = cmd.Amount,
                AssetId = cmd.AssetId,
                Type = HistoryOperationType.CashOut,
                CreatedOn = DateTime.UtcNow,
                TxHash = cmd.TransactionHash,
                MerchantId = merchantWallet.MerchantId,
                DesiredAssetId = cmd.DesiredAsset,
                EmployeeEmail = cmd.EmployeeEmail
            };

            await _publisherRetryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(operation));

            return operation.Id;
        }

        public async Task SetTxHashAsync(string id, string hash)
        {
            await _clientRetryPolicy
                .ExecuteAsync(() => _payHistoryClient.SetTxHashAsync(id, hash));
        }

        public async Task RemoveAsync(string id)
        {
            await _clientRetryPolicy
                .ExecuteAsync(() => _payHistoryClient.SetRemovedAsync(id));
        }
    }
}
