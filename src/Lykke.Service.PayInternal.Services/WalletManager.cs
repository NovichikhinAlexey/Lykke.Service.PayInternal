using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
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
        private readonly IIndex<BlockchainType, IBlockchainApiClient> _blockchainClients;
        private readonly IList<BlockchainWalletAllocationPolicy> _walletAllocationSettings;
        private readonly IBcnWalletUsageService _bcnWalletUsageService;
        private readonly IWalletEventsPublisher _walletEventsPublisher;
        private readonly IPaymentRequestTransactionRepository _blockchainTransactionRepository;

        private const int BatchPieceSize = 15;

        public WalletManager(
            IVirtualWalletService virtualWalletService,
            IIndex<BlockchainType, IBlockchainApiClient> blockchainClients,
            IList<BlockchainWalletAllocationPolicy> walletAllocationSettings, 
            IBcnWalletUsageService bcnWalletUsageService, 
            IWalletEventsPublisher walletEventsPublisher, 
            IPaymentRequestTransactionRepository blockchainTransactionRepository)
        {
            _virtualWalletService = virtualWalletService;
            _blockchainClients = blockchainClients;
            _walletAllocationSettings = walletAllocationSettings;
            _bcnWalletUsageService = bcnWalletUsageService;
            _walletEventsPublisher = walletEventsPublisher;
            _blockchainTransactionRepository = blockchainTransactionRepository;
        }

        public async Task<IVirtualWallet> CreateAsync(string merchantId, DateTime dueDate, string assetId = null)
        {
            IVirtualWallet wallet = await _virtualWalletService.CreateAsync(merchantId, dueDate);

            if (assetId != null)
            {
                wallet = await AddAssetAsync(wallet.MerchantId, wallet.Id, assetId);
            }

            return wallet;
        }

        public async Task<IVirtualWallet> AddAssetAsync(string merchantId, string walletId, string assetId)
        {
            IVirtualWallet virtualWallet = await _virtualWalletService.GetAsync(merchantId, walletId);

            if (virtualWallet == null)
                throw new WalletNotFoundException(walletId);

            BlockchainType blockchainType;

            switch (assetId)
            {
                case LykkeConstants.BitcoinAssetId:
                case LykkeConstants.SatoshiAssetId:
                    blockchainType = BlockchainType.Bitcoin;
                    break;
                default: throw new AssetNotSupportedException(assetId);
            }

            if (!_blockchainClients.TryGetValue(blockchainType, out IBlockchainApiClient blockchainClient))
                throw new InvalidOperationException($"Blockchain client of type [{blockchainType}] not found");

            BlockchainWalletAllocationPolicy walletAllocationSetting =
                _walletAllocationSettings.SingleOrDefault(x => x.Blockchain == blockchainType);

            WalletAllocationPolicy policy =
                walletAllocationSetting?.WalletAllocationPolicy ?? WalletAllocationPolicy.New;

            IVirtualWallet updatedWallet;

            switch (policy)
            {
                case WalletAllocationPolicy.New:
                    string address = await blockchainClient.CreateAddress();

                    await _bcnWalletUsageService.OccupyAsync(address, blockchainType, virtualWallet.Id);

                    updatedWallet = await _virtualWalletService.AddAddressAsync(
                        virtualWallet.MerchantId, 
                        virtualWallet.Id,
                        new BlockchainWallet
                        {
                            AssetId = assetId,
                            Address = address,
                            Blockchain = blockchainType
                        });
                    //todo: update to take into account blockchain, now address is considered as bitcoin blockchain address
                    await _walletEventsPublisher.PublishAsync(new Wallet
                    {
                        Address = address,
                        DueDate = virtualWallet.DueDate
                    });

                    break;
                case WalletAllocationPolicy.Reuse:
                    //todo: try to lock vacant wallet
                    throw  new NotImplementedException();

                    break;
                default:
                    throw new UnknownWalletAllocationPolicyException(policy.ToString());
            }

            return updatedWallet;
        }

        public async Task<IEnumerable<IWalletState>> GetNotExpiredStateAsync()
        {
            IReadOnlyList<IVirtualWallet> wallets = await _virtualWalletService.GetNotExpiredAsync();

            var transactions = new List<IPaymentRequestTransaction>();

            foreach (IEnumerable<IVirtualWallet> batch in wallets.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(x => _blockchainTransactionRepository.GetAsync(x.Id)
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
                    Transactions = transactions.Where(t =>
                        t.WalletAddress == virtualWallet.Id && t.Blockchain == bcnWallet.Blockchain)
                }));
            }

            return walletStateResult;
        }
    }
}
