using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantWalletService : IMerchantWalletService
    {
        private readonly IMerchantWalletRespository _merchantWalletRespository;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILog _log;

        public MerchantWalletService(
            [NotNull] IMerchantWalletRespository merchantWalletRespository,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] IBlockchainClientProvider blockchainClientProvider,
            [NotNull] ILog log, 
            [NotNull] IAssetsLocalCache assetsLocalCache)
        {
            _merchantWalletRespository = merchantWalletRespository ??
                                         throw new ArgumentNullException(nameof(merchantWalletRespository));
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _blockchainClientProvider = blockchainClientProvider ??
                                        throw new ArgumentNullException(nameof(blockchainClientProvider));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _log = log.CreateComponentScope(nameof(MerchantWalletService)) ??
                   throw new ArgumentNullException(nameof(log));
        }

        public async Task<IMerchantWallet> CreateAsync(CreateMerchantWalletCommand cmd)
        {
            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(cmd.Network);

            string walletAddress = await blockchainClient.CreateAddressAsync();

            return await _merchantWalletRespository.CreateAsync(new MerchantWallet
            {
                MerchantId = cmd.MerchantId,
                Network = cmd.Network,
                WalletAddress = walletAddress,
                CreatedOn = DateTime.UtcNow,
                DisplayName = cmd.DisplayName,
                IncomingPaymentDefaults = Enumerable.Empty<string>().ToList(),
                OutgoingPaymentDefaults = Enumerable.Empty<string>().ToList()
            });
        }

        public async Task DeleteAsync(string merchantId, BlockchainType network, string walletAddress)
        {
            try
            {
                await _merchantWalletRespository.DeleteAsync(merchantId, network, walletAddress);
            }
            catch (KeyNotFoundException e)
            {
                _log.WriteError(nameof(DeleteAsync), new
                {
                    merchantId,
                    network,
                    walletAddress
                }, e);

                throw new MerchantWalletNotFoundException(merchantId, network, walletAddress);
            }
        }

        public async Task DeleteAsync(string merchantWalletId)
        {
            try
            {
                await _merchantWalletRespository.DeleteAsync(merchantWalletId);
            }
            catch (KeyNotFoundException e)
            {
                _log.WriteError(nameof(DeleteAsync), new {merchantWalletId}, e);

                throw new MerchantWalletIdNotFoundException(merchantWalletId);
            }
        }

        public async Task SetDefaultAssetsAsync(
            string merchantId,
            BlockchainType network,
            string walletAddress,
            IEnumerable<string> incomingPaymentDefaults = null,
            IEnumerable<string> outgoingPaymentDefaults = null)
        {
            try
            {
                await _merchantWalletRespository.UpdateAsync(new MerchantWallet
                {
                    MerchantId = merchantId,
                    Network = network,
                    WalletAddress = walletAddress,
                    IncomingPaymentDefaults = incomingPaymentDefaults?.ToList(),
                    OutgoingPaymentDefaults = outgoingPaymentDefaults?.ToList(),
                });
            }
            catch (KeyNotFoundException e)
            {
                _log.WriteError(nameof(SetDefaultAssetsAsync), new
                {
                    merchantId,
                    network,
                    walletAddress
                }, e);

                throw new MerchantWalletNotFoundException(merchantId, network, walletAddress);
            }
        }

        public async Task<IMerchantWallet> GetDefaultAsync(string merchantId, string assetId,
            PaymentDirection paymentDirection)
        {
            BlockchainType network = await _assetSettingsService.GetNetworkAsync(assetId);

            IReadOnlyList<IMerchantWallet> merchantWallets =
                (await _merchantWalletRespository.GetByMerchantAsync(merchantId)).Where(x => x.Network == network)
                .ToList();

            string assetDisplayId =
                assetId.IsGuid() ? (await _assetsLocalCache.GetAssetByIdAsync(assetId)).DisplayId : assetId;

            IReadOnlyList<IMerchantWallet> assetDefaultWallets = merchantWallets
                .Where(w => w.GetDefaultAssets(paymentDirection).Contains(assetDisplayId))
                .ToList();

            if (assetDefaultWallets.MoreThanOne())
                throw new MultipleDefaultMerchantWalletsException(merchantId, assetId, paymentDirection);

            if (assetDefaultWallets.Any())
                return assetDefaultWallets.Single();

            IReadOnlyList<IMerchantWallet> anyAssetDefaultWallets = merchantWallets
                .Where(w => !w.GetDefaultAssets(paymentDirection).Any())
                .ToList();

            if (anyAssetDefaultWallets.MoreThanOne())
                throw new MultipleDefaultMerchantWalletsException(merchantId, assetId, paymentDirection);

            if (anyAssetDefaultWallets.Any())
                return anyAssetDefaultWallets.Single();

            throw new DefaultMerchantWalletNotFoundException(merchantId, assetId, paymentDirection);
        }

        public Task<IReadOnlyList<IMerchantWallet>> GetByMerchantAsync(string merchantId)
        {
            return _merchantWalletRespository.GetByMerchantAsync(merchantId);
        }

        public async Task<IMerchantWallet> GetByIdAsync(string merchantWalletId)
        {
            try
            {
                return await _merchantWalletRespository.GetByIdAsync(merchantWalletId);
            }
            catch (KeyNotFoundException e)
            {
                _log.WriteError(nameof(GetByIdAsync), new {merchantWalletId}, e);

                throw new MerchantWalletIdNotFoundException(merchantWalletId);
            }
        }

        public async Task<IMerchantWallet> GetByAddressAsync(BlockchainType network, string walletAddress)
        {
            try
            {
                return await _merchantWalletRespository.GetByAddressAsync(network, walletAddress);
            }
            catch (KeyNotFoundException e)
            {
                _log.WriteError(nameof(GetByAddressAsync), new { network, walletAddress }, e);

                throw new MerchantWalletNotFoundException(string.Empty, network, walletAddress);
            }
        }

        public async Task<IReadOnlyList<MerchantWalletBalanceLine>> GetBalancesAsync(string merchantId)
        {
            var balances = new List<MerchantWalletBalanceLine>();

            IReadOnlyList<IMerchantWallet> wallets = await _merchantWalletRespository.GetByMerchantAsync(merchantId);

            foreach (IMerchantWallet merchantWallet in wallets)
            {
                IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(merchantWallet.Network);

                IReadOnlyList<BlockchainBalanceResult> walletBalance =
                    await blockchainClient.GetBalancesAsync(merchantWallet.WalletAddress);

                balances.AddRange(walletBalance.Select(x => new MerchantWalletBalanceLine
                {
                    Id = merchantWallet.Id,
                    AssetId = x.AssetId,
                    Balance = x.Balance
                }));
            }

            return balances;
        }
    }
}
