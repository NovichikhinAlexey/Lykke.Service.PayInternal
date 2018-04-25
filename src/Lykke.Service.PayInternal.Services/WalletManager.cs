using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
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
        private readonly ILog _log;

        private const int BatchPieceSize = 15;

        public WalletManager(
            IVirtualWalletService virtualWalletService,
            IList<BlockchainWalletAllocationPolicy> walletAllocationSettings,
            IBcnWalletUsageService bcnWalletUsageService,
            IWalletEventsPublisher walletEventsPublisher,
            IBlockchainClientProvider blockchainClientProvider, 
            ITransactionsService transactionsService, 
            ILog log)
        {
            _virtualWalletService = virtualWalletService;
            _walletAllocationSettings = walletAllocationSettings;
            _bcnWalletUsageService = bcnWalletUsageService;
            _walletEventsPublisher = walletEventsPublisher;
            _blockchainClientProvider = blockchainClientProvider;
            _transactionsService = transactionsService;
            _log = log;
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

            BlockchainType blockchainType = assetId.GetBlockchainType();

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
