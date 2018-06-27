using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Exchange;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly IAssetRatesService _assetRatesService;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBcnSettingsResolver _bcnSettingsResolver;
        private readonly ITransferService _transferService;
        private readonly ITransactionsService _transactionsService;

        public ExchangeService(
            [NotNull] IMerchantWalletService merchantWalletService,
            [NotNull] IBlockchainClientProvider blockchainClientProvider,
            [NotNull] IAssetRatesService assetRatesService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] IBcnSettingsResolver bcnSettingsResolver,
            [NotNull] ITransferService transferService,
            [NotNull] ITransactionsService transactionsService)
        {
            _merchantWalletService =
                merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _blockchainClientProvider = blockchainClientProvider ??
                                        throw new ArgumentNullException(nameof(blockchainClientProvider));
            _assetRatesService = assetRatesService ?? throw new ArgumentNullException(nameof(assetRatesService));
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _bcnSettingsResolver = bcnSettingsResolver ?? throw new ArgumentNullException(nameof(bcnSettingsResolver));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
        }

        public async Task<ExchangeResult> ExecuteAsync(ExchangeCommand cmd)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(cmd.SourceAssetId);

            if (await _assetSettingsService.GetNetworkAsync(cmd.DestAssetId) != network)
                throw new ExchangeOperationNotSupportedException();

            string hotwallet = _bcnSettingsResolver.GetExchangeHotWallet(network);

            IBlockchainApiClient blockchainApiClient = _blockchainClientProvider.Get(network);

            decimal hotwalletBalance = await blockchainApiClient.GetBalanceAsync(hotwallet, cmd.DestAssetId);

            IAssetPairRate rate = await _assetRatesService.GetCurrentRate(cmd.SourceAssetId, cmd.DestAssetId);

            decimal exchangeAmount = cmd.SourceAmount * rate.BidPrice;

            if (hotwalletBalance < exchangeAmount)
                throw new InsufficientFundsException(hotwallet, cmd.DestAssetId);

            TransferResult toHotWallet = await _transferService.ExchangeThrowFail(
                cmd.SourceAssetId,
                await GetSourceAddressAsync(cmd),
                hotwallet,
                cmd.SourceAmount);

            await RegisterTransferTxsAsync(toHotWallet);

            TransferResult fromHotWallet = await _transferService.ExchangeThrowFail(
                cmd.DestAssetId,
                hotwallet,
                await GetDestWalletAddressAsync(cmd),
                exchangeAmount);

            await RegisterTransferTxsAsync(fromHotWallet);

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

            return merchantWallet?.WalletAddress;
        }

        private async Task<string> GetDestWalletAddressAsync(ExchangeCommand cmd)
        {
            IMerchantWallet merchantWallet = await GetExchangeWalletAsync(cmd, PaymentDirection.Incoming);

            return merchantWallet?.WalletAddress;
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

        private async Task RegisterTransferTxsAsync(TransferResult transfer)
        {
            foreach (var transferTransactionResult in transfer.Transactions)
            {
                await _transactionsService.CreateTransactionAsync(new CreateTransactionCommand
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
                    SourceWalletAddresses = transferTransactionResult.Sources.ToArray()
                });
            }
        }
    }
}
