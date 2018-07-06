using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayHistory.Client.AutorestClient.Models;
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
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly RetryPolicy _retryPolicy;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly RetryPolicySettings _retryPolicySettings;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ILog _log;

        public WalletHistoryService(
            [NotNull] HistoryOperationPublisher historyOperationPublisher, 
            [NotNull] IMerchantWalletService merchantWalletService, 
            [NotNull] ILog log, 
            [NotNull] RetryPolicySettings retryPolicySettings)
        {
            _historyOperationPublisher = historyOperationPublisher ?? throw new ArgumentNullException(nameof(historyOperationPublisher));
            _merchantWalletService = merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _retryPolicySettings = retryPolicySettings ?? throw new ArgumentNullException(nameof(retryPolicySettings));
            _log = log.CreateComponentScope(nameof(WalletHistoryService)) ?? throw new ArgumentNullException(nameof(log));
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.Error("Publish wallet history with retry", ex));
        }

        public async Task PublishCashInAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            await _retryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(new HistoryOperation
                {
                    Amount = cmd.Amount,
                    AssetId = cmd.AssetId,
                    Type = HistoryOperationType.Recharge,
                    CreatedOn = DateTime.UtcNow,
                    TxHash = cmd.TransactionHash,
                    MerchantId = merchantWallet.MerchantId
                }));
        }

        public async Task PublishOutgoingExchangeAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            await _retryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(new HistoryOperation
                {
                    Amount = cmd.Amount,
                    AssetId = cmd.AssetId,
                    Type = HistoryOperationType.OutgoingExchange,
                    CreatedOn = DateTime.UtcNow,
                    TxHash = cmd.TransactionHash,
                    MerchantId = merchantWallet.MerchantId,
                }));
        }

        public async Task PublishIncomingExchangeAsync(IWalletHistoryCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            await _retryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(new HistoryOperation
                {
                    Amount = cmd.Amount,
                    AssetId = cmd.AssetId,
                    Type = HistoryOperationType.IncomingExchange,
                    CreatedOn = DateTime.UtcNow,
                    TxHash = cmd.TransactionHash,
                    MerchantId = merchantWallet.MerchantId
                }));
        }

        public async Task PublishCashoutAsync(IWalletHistoryCashoutCommand cmd)
        {
            IMerchantWallet merchantWallet =
                await _merchantWalletService.GetByAddressAsync(cmd.Blockchain, cmd.WalletAddress);

            await _retryPolicy
                .ExecuteAsync(() => _historyOperationPublisher.PublishAsync(new HistoryOperation
                {
                    Amount = cmd.Amount,
                    AssetId = cmd.AssetId,
                    Type = HistoryOperationType.CashOut,
                    CreatedOn = DateTime.UtcNow,
                    TxHash = cmd.TransactionHash,
                    MerchantId = merchantWallet.MerchantId,
                    DesiredAssetId = cmd.DesiredAsset,
                    EmployeeEmail = cmd.EmployeeEmail
                }));
        }
    }
}
