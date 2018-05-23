using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;
using MoreLinq;

namespace Lykke.Service.PayInternal.Services
{
    public class WalletManager : IWalletManager
    {
        private readonly IVirtualWalletService _virtualWalletService;
        private readonly IList<BlockchainWalletAllocationPolicy> _walletAllocationSettings;
        private readonly IBcnWalletUsageService _bcnWalletUsageService;
        private readonly IWalletEventsPublisher _walletEventsPublisher;
        private readonly ITransactionsService _transactionsService;
        private readonly IBlockchainClientProvider _blockchainClientProvider;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly ILog _log;

        private const int BatchPieceSize = 15;

        public WalletManager(
            [NotNull] IVirtualWalletService virtualWalletService,
            [NotNull] IList<BlockchainWalletAllocationPolicy> walletAllocationSettings,
            [NotNull] IBcnWalletUsageService bcnWalletUsageService,
            [NotNull] IWalletEventsPublisher walletEventsPublisher,
            [NotNull] IBlockchainClientProvider blockchainClientProvider,
            [NotNull] ITransactionsService transactionsService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] ILog log)
        {
            _virtualWalletService = virtualWalletService ?? throw new ArgumentNullException(nameof(virtualWalletService));
            _walletAllocationSettings = walletAllocationSettings ?? throw new ArgumentNullException(nameof(walletAllocationSettings));
            _bcnWalletUsageService = bcnWalletUsageService ?? throw new ArgumentNullException(nameof(bcnWalletUsageService));
            _walletEventsPublisher = walletEventsPublisher ?? throw new ArgumentNullException(nameof(walletEventsPublisher));
            _blockchainClientProvider = blockchainClientProvider ?? throw new ArgumentNullException(nameof(blockchainClientProvider));
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IVirtualWallet> CreateAsync(string merchantId, DateTime dueDate, string assetId = null)
        {
            IVirtualWallet wallet = await _virtualWalletService.CreateAsync(merchantId, dueDate);

            if (assetId != null)
            {
                wallet = await AddAssetAsync(wallet.MerchantId, wallet.Id, assetId);
            }

            await _log.WriteInfoAsync(nameof(WalletManager), nameof(CreateAsync), wallet.ToJson(),
                "New virtual wallet created");

            return wallet;
        }

        public async Task<IVirtualWallet> AddAssetAsync(string merchantId, string walletId, string assetId)
        {
            IVirtualWallet virtualWallet = await _virtualWalletService.GetAsync(merchantId, walletId);

            if (virtualWallet == null)
                throw new WalletNotFoundException(walletId);

            BlockchainType blockchainType = await _assetSettingsService.GetNetworkAsync(assetId);

            IBlockchainApiClient blockchainClient = _blockchainClientProvider.Get(blockchainType);

            WalletAllocationPolicy policy = _walletAllocationSettings.GetPolicy(blockchainType);

            IBcnWalletUsage walletUsage;

            switch (policy)
            {
                case WalletAllocationPolicy.New:
                    string address = await blockchainClient.CreateAddressAsync();

                    walletUsage = await _bcnWalletUsageService.OccupyAsync(address, blockchainType, virtualWallet.Id);

                    break;
                case WalletAllocationPolicy.Reuse:
                    try
                    {
                        walletUsage = await _bcnWalletUsageService.OccupyAsync(blockchainType, virtualWallet.Id);
                    }
                    catch (WalletAddressAllocationException)
                    {
                        string newAddress = await blockchainClient.CreateAddressAsync();

                        walletUsage =
                            await _bcnWalletUsageService.OccupyAsync(newAddress, blockchainType, virtualWallet.Id);
                    }

                    break;
                default:
                    throw new UnknownWalletAllocationPolicyException(policy.ToString());
            }

            IVirtualWallet updatedWallet = await _virtualWalletService.AddAddressAsync(
                virtualWallet.MerchantId,
                virtualWallet.Id,
                new BlockchainWallet
                {
                    AssetId = assetId,
                    Address = walletUsage.WalletAddress,
                    Blockchain = walletUsage.Blockchain
                });

            await _walletEventsPublisher.PublishAsync(walletUsage.WalletAddress, blockchainType, virtualWallet.DueDate);

            return updatedWallet;
        }

        public async Task<IVirtualWallet> EnsureBcnAddressAllocated(string merchantId, string walletId, string assetId)
        {
            IVirtualWallet virtualWallet = await _virtualWalletService.GetAsync(merchantId, walletId);

            if (virtualWallet == null)
                throw new WalletNotFoundException(walletId);

            BlockchainType blockchainType = await _assetSettingsService.GetNetworkAsync(assetId);

            if (virtualWallet.BlockchainWallets.Any(x => x.Blockchain == blockchainType))
                return virtualWallet;

            return await AddAssetAsync(merchantId, walletId, assetId);
        }

        public async Task<IEnumerable<IWalletState>> GetNotExpiredStateAsync()
        {
            IReadOnlyList<IVirtualWallet> wallets = await _virtualWalletService.GetNotExpiredAsync();

            var transactions = new List<IPaymentRequestTransaction>();

            foreach (IEnumerable<IVirtualWallet> batch in wallets.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(x => _transactionsService.GetByWalletAsync(x.Id)
                    .ContinueWith(t =>
                    {
                        lock (transactions)
                        {
                            transactions.AddRange(t.Result);
                        }
                    })));
            }

            var walletStateResult = new List<WalletState>();

            foreach (IVirtualWallet virtualWallet in wallets)
            {
                walletStateResult.AddRange(virtualWallet.BlockchainWallets.Select(bcnWallet => new WalletState
                {
                    DueDate = virtualWallet.DueDate,
                    Address = bcnWallet.Address,
                    Blockchain = bcnWallet.Blockchain,
                    Transactions = transactions.Where(t =>
                        t.WalletAddress == virtualWallet.Id && t.Blockchain == bcnWallet.Blockchain)
                }));
            }

            return walletStateResult;
        }

        public async Task<string> ResolveBlockchainAddressAsync(string virtualAddress, string assetId)
        {
            IVirtualWallet wallet = await _virtualWalletService.FindAsync(virtualAddress);

            if (wallet == null)
                throw new WalletNotFoundException(virtualAddress);

            BlockchainWallet bcnWallet =
                wallet.BlockchainWallets.SingleOrDefault(x => x.AssetId == assetId);

            return bcnWallet?.Address;
        }
    }
}
