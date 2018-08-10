using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Exchange;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Context;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Polly;
using LogExtensions = Lykke.Service.PayInternal.Core.LogExtensions;

namespace Lykke.Service.PayInternal.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly IAssetRatesService _assetRatesService;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBcnSettingsResolver _bcnSettingsResolver;
        private readonly ITransferService _transferService;
        private readonly ITransactionsService _transactionsService;
        private readonly IWalletBalanceValidator _walletBalanceValidator;
        private readonly IWalletHistoryService _walletHistoryService;
        private readonly Policy _retryPolicy;
        private readonly ILog _log;

        public ExchangeService(
            [NotNull] IMerchantWalletService merchantWalletService,
            [NotNull] IAssetRatesService assetRatesService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] IBcnSettingsResolver bcnSettingsResolver,
            [NotNull] ITransferService transferService,
            [NotNull] ITransactionsService transactionsService, 
            [NotNull] IWalletBalanceValidator walletBalanceValidator, 
            [NotNull] IWalletHistoryService walletHistoryService,
            [NotNull] RetryPolicySettings retryPolicySettings,
            [NotNull] ILogFactory logFactory)
        {
            _merchantWalletService =
                merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _assetRatesService = assetRatesService ?? throw new ArgumentNullException(nameof(assetRatesService));
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _bcnSettingsResolver = bcnSettingsResolver ?? throw new ArgumentNullException(nameof(bcnSettingsResolver));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _walletBalanceValidator = walletBalanceValidator ?? throw new ArgumentNullException(nameof(walletBalanceValidator));
            _walletHistoryService = walletHistoryService ?? throw new ArgumentNullException(nameof(walletHistoryService));
            _log = logFactory.CreateLog(this);
            _retryPolicy = Policy
                .Handle<InsufficientFundsException>()
                .Or<ExchangeOperationFailedException>()
                .Or<ExchangeOperationPartiallyFailedException>()
                .WaitAndRetryAsync(
                    retryPolicySettings.DefaultAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timespan) => _log.Error(ex, "Exchange with retry"));
        }

        public async Task<ExchangeResult> ExecuteAsync(ExchangeCommand cmd)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(cmd.SourceAssetId);

            if (await _assetSettingsService.GetNetworkAsync(cmd.DestAssetId) != network)
                throw new ExchangeOperationNotSupportedException("Assets are being served by different blockchains");

            IAssetPairRate rate = await _assetRatesService.GetCurrentRate(cmd.SourceAssetId, cmd.DestAssetId);

            if (cmd.ExpectedRate != null && rate.BidPrice != cmd.ExpectedRate)
            {
                throw new ExchangeRateChangedException(rate.BidPrice);
            }

            string hotwallet = _bcnSettingsResolver.GetExchangeHotWallet(network);

            string sourceAddress = await GetSourceAddressAsync(cmd);

            decimal exchangeAmount = cmd.SourceAmount * rate.BidPrice;

            await _walletBalanceValidator.ValidateTransfer(sourceAddress, cmd.SourceAssetId, cmd.SourceAmount);

            await _walletBalanceValidator.ValidateTransfer(hotwallet, cmd.DestAssetId, exchangeAmount);

            TransferResult toHotWallet = await _retryPolicy
                .ExecuteAsync(() => _transferService.ExchangeThrowFail(
                    cmd.SourceAssetId,
                    sourceAddress,
                    hotwallet,
                    cmd.SourceAmount));

            await RegisterTransferTxsAsync(toHotWallet);

            string destAddress = await GetDestAddressAsync(cmd);

            TransferResult fromHotWallet = await _retryPolicy
                .ExecuteAsync(() => _transferService.ExchangeThrowFail(
                    cmd.DestAssetId,
                    hotwallet,
                    destAddress,
                    exchangeAmount));

            await RegisterTransferTxsAsync(fromHotWallet, false);

            return new ExchangeResult
            {
                SourceAssetId = cmd.SourceAssetId,
                SourceAmount = cmd.SourceAmount,
                DestAssetId = cmd.DestAssetId,
                DestAmount = exchangeAmount,
                Rate = rate.BidPrice
            };
        }

        private async Task<string> GetSourceAddressAsync(ExchangeCommand cmd)
        {
            IMerchantWallet merchantWallet = await GetExchangeWalletAsync(cmd, PaymentDirection.Outgoing);

            if (merchantWallet.MerchantId != cmd.MerchantId)
                throw new MerchantWalletOwnershipException(cmd.MerchantId, merchantWallet.WalletAddress);

            return merchantWallet.WalletAddress;
        }

        private async Task<string> GetDestAddressAsync(ExchangeCommand cmd)
        {
            IMerchantWallet merchantWallet = await GetExchangeWalletAsync(cmd, PaymentDirection.Incoming);

            if (merchantWallet.MerchantId != cmd.MerchantId)
                throw new MerchantWalletOwnershipException(cmd.MerchantId, merchantWallet.WalletAddress);

            return merchantWallet.WalletAddress;
        }

        private async Task<IMerchantWallet> GetExchangeWalletAsync(ExchangeCommand cmd,
            PaymentDirection paymentDirection)
        {
            string merchantWalletId = cmd.GetWalletId(paymentDirection);

            string assetId = cmd.GetAssetId(paymentDirection);

            return string.IsNullOrEmpty(merchantWalletId)
                ? await _merchantWalletService.GetDefaultAsync(cmd.MerchantId, assetId, paymentDirection)
                : await _merchantWalletService.GetByIdAsync(merchantWalletId);
        }

        private async Task RegisterTransferTxsAsync(TransferResult transfer, bool outgoing = true)
        {
            foreach (var transferTransactionResult in transfer.Transactions)
            {
                string historyId = outgoing
                    ? await _walletHistoryService.PublishOutgoingExchangeAsync(Mapper
                        .Map<WalletHistoryOutgoingExchangeCommand>(transferTransactionResult).Map(transfer))
                    : await _walletHistoryService.PublishIncomingExchangeAsync(Mapper
                        .Map<WalletHistoryIncomingExchangeCommand>(transferTransactionResult).Map(transfer));

                await _transactionsService.CreateTransactionAsync(
                    new CreateTransactionCommand
                    {
                        Amount = transferTransactionResult.Amount,
                        Blockchain = transfer.Blockchain,
                        AssetId = transferTransactionResult.AssetId,
                        Confirmations = 0,
                        Hash = transferTransactionResult.Hash,
                        IdentityType = transferTransactionResult.IdentityType,
                        Identity = transferTransactionResult.Identity,
                        TransferId = transfer.Id,
                        Type = TransactionType.Exchange,
                        SourceWalletAddresses = transferTransactionResult.Sources.ToArray(),
                        ContextData = new ExchangeTransactonContext {HistoryOperationId = historyId}.ToJson()
                    });
            }
        }

        public async Task<ExchangeResult> PreExchangeAsync(PreExchangeCommand cmd)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(cmd.SourceAssetId);

            if (await _assetSettingsService.GetNetworkAsync(cmd.DestAssetId) != network)
                throw new ExchangeOperationNotSupportedException("Assets are being served by different blockchains");

            string sourceWalletAddress = (await _merchantWalletService.GetDefaultAsync(
                cmd.MerchantId,
                cmd.SourceAssetId,
                PaymentDirection.Outgoing)).WalletAddress;

            await _walletBalanceValidator.ValidateTransfer(sourceWalletAddress, cmd.SourceAssetId, cmd.SourceAmount);

            string hotwallet = _bcnSettingsResolver.GetExchangeHotWallet(network);

            IAssetPairRate rate = await _assetRatesService.GetCurrentRate(cmd.SourceAssetId, cmd.DestAssetId);

            decimal exchangeAmount = cmd.SourceAmount * rate.BidPrice;

            await _walletBalanceValidator.ValidateTransfer(hotwallet, cmd.DestAssetId, exchangeAmount);

            return new ExchangeResult
            {
                SourceAssetId = cmd.SourceAssetId,
                SourceAmount = cmd.SourceAmount,
                DestAssetId = cmd.DestAssetId,
                DestAmount = exchangeAmount,
                Rate = rate.BidPrice
            };
        }
    }
}
