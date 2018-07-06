using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Cashout;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class CashoutService : ICashoutService
    {
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBcnSettingsResolver _bcnSettingsResolver;
        private readonly ITransferService _transferService;
        private readonly ITransactionsService _transactionsService;
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly IWalletBalanceValidator _walletBalanceValidator;

        public CashoutService(
            [NotNull] IAssetSettingsService assetSettingsService, 
            [NotNull] IBcnSettingsResolver bcnSettingsResolver, 
            [NotNull] ITransferService transferService, 
            [NotNull] ITransactionsService transactionsService, 
            [NotNull] IMerchantWalletService merchantWalletService, 
            [NotNull] IWalletBalanceValidator walletBalanceValidator)
        {
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _bcnSettingsResolver = bcnSettingsResolver ?? throw new ArgumentNullException(nameof(bcnSettingsResolver));
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _merchantWalletService = merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _walletBalanceValidator = walletBalanceValidator ?? throw new ArgumentNullException(nameof(walletBalanceValidator));
        }

        public async Task<CashoutResult> ExecuteAsync(CashoutCommand cmd)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(cmd.SourceAssetId);

            string hotwallet = _bcnSettingsResolver.GetCashoutHotWallet(network);

            if (string.IsNullOrEmpty(hotwallet))
                throw new CashoutHotwalletNotDefinedException(network);

            string sourceAddress = await GetSourceAddressAsync(cmd);

            await _walletBalanceValidator.ValidateTransfer(sourceAddress, cmd.SourceAssetId, cmd.SourceAmount);

            TransferResult toHotWallet = await _transferService.CashoutThrowFail(
                cmd.SourceAssetId,
                sourceAddress,
                hotwallet,
                cmd.SourceAmount);

            foreach (var transferTransactionResult in toHotWallet.Transactions)
            {
                await _transactionsService.CreateTransactionAsync(new CreateTransactionCommand
                {
                    Amount = transferTransactionResult.Amount,
                    Blockchain = toHotWallet.Blockchain,
                    AssetId = transferTransactionResult.AssetId,
                    Confirmations = 0,
                    Hash = transferTransactionResult.Hash,
                    IdentityType = transferTransactionResult.IdentityType,
                    Identity = transferTransactionResult.Identity,
                    TransferId = toHotWallet.Id,
                    Type = TransactionType.CashOut,
                    SourceWalletAddresses = transferTransactionResult.Sources.ToArray(),
                    ContextData = cmd.GetContext().ToJson()
                });
            }

            return new CashoutResult
            {
                Amount = cmd.SourceAmount,
                AssetId = cmd.SourceAssetId,
                SourceWalletAddress = sourceAddress,
                DestWalletAddress = hotwallet
            };
        }

        private async Task<string> GetSourceAddressAsync(CashoutCommand cmd)
        {
            IMerchantWallet merchantWallet = await GetCashoutWalletAsync(cmd);

            if (merchantWallet.MerchantId != cmd.MerchantId)
                throw new MerchantWalletOwnershipException(cmd.MerchantId, merchantWallet.WalletAddress);

            return merchantWallet.WalletAddress;
        }

        private async Task<IMerchantWallet> GetCashoutWalletAsync(CashoutCommand cmd)
        {
            string merchantWalletId = cmd.SourceMerchantWalletId;

            return string.IsNullOrEmpty(merchantWalletId)
                ? await _merchantWalletService.GetDefaultAsync(cmd.MerchantId, cmd.SourceAssetId, PaymentDirection.Outgoing)
                : await _merchantWalletService.GetByIdAsync(merchantWalletId);
        }
    }
}
