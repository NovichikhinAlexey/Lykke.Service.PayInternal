using System;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class WalletBalanceValidator : IWalletBalanceValidator
    {
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;

        public WalletBalanceValidator(
            [NotNull] IAssetSettingsService assetSettingsService, 
            [NotNull] IBlockchainClientProvider blockchainClientProvider, 
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        public async Task ValidateTransfer(string walletAddress, string assetId, decimal transferAmount)
        {
            assetId = assetId.IsGuid() ? assetId : await _lykkeAssetsResolver.GetLykkeId(assetId);

            BlockchainType network = await _assetSettingsService.GetNetworkAsync(assetId);

            IBlockchainApiClient blockchainApiClient = _blockchainClientProvider.Get(network);

            decimal balance = await blockchainApiClient.GetBalanceAsync(walletAddress, assetId);

            if (balance < transferAmount)
                throw new InsufficientFundsException(walletAddress, assetId);
        }
    }
}
