using System;
using System.Threading.Tasks;
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

        public WalletBalanceValidator(
            [NotNull] IAssetSettingsService assetSettingsService, 
            [NotNull] IBlockchainClientProvider blockchainClientProvider)
        {
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
        }

        public async Task ValidateTransfer(string walletAddress, string assetId, decimal transferAmount)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(assetId);

            IBlockchainApiClient blockchainApiClient = _blockchainClientProvider.Get(network);

            decimal balance = await blockchainApiClient.GetBalanceAsync(walletAddress, assetId);

            if (balance < transferAmount)
                throw new InsufficientFundsException(walletAddress, assetId);
        }
    }
}
