using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using MoreLinq;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantWalletsService : IMerchantWalletsService
    {
        private readonly IBitcoinApiClient _bitcoinServiceClient;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletEventsPublisher _walletEventsPublisher;
        private readonly IBlockchainTransactionRepository _blockchainTransactionRepository;

        private const int BatchPieceSize = 15;

        public MerchantWalletsService(
            IBitcoinApiClient bitcoinServiceClient,
            IWalletRepository walletRepository,
            IWalletEventsPublisher walletEventsPublisher,
            IBlockchainTransactionRepository blockchainTransactionRepository)
        {
            _bitcoinServiceClient =
                bitcoinServiceClient ?? throw new ArgumentNullException(nameof(bitcoinServiceClient));
            _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            _walletEventsPublisher =
                walletEventsPublisher ?? throw new ArgumentNullException(nameof(walletEventsPublisher));
            _blockchainTransactionRepository = blockchainTransactionRepository ??
                                               throw new ArgumentNullException(nameof(blockchainTransactionRepository));
        }

        public async Task<string> CreateAddress(ICreateWalletRequest request)
        {
            var response = await _bitcoinServiceClient.GenerateLykkePayWallet();

            if (response.HasError)
            {
                throw new Exception(response.Error?.ToJson());
            }

            var walletEntity = new WalletEntity
            {
                Address = response.Address,
                MerchantId = request.MerchantId,
                Amount = default(double),
                DueDate = request.DueDate,
                PublicKey = response.PubKey
            };

            await _walletRepository.SaveAsync(walletEntity);

            await _walletEventsPublisher.PublishAsync(walletEntity.ToNewWalletMessage());

            return response.Address;
        }

        public async Task<IWallet> GetAsync(string merchantId, string address)
        {
            return await _walletRepository.GetAsync(merchantId, address);
        }

        public async Task<IEnumerable<IWallet>> GetAsync(string merchantId)
        {
            return await _walletRepository.GetByMerchantAsync(merchantId);
        }

        public async Task<IEnumerable<IWallet>> GetNonEmptyAsync(string merchantId)
        {
            return await _walletRepository.GetByMerchantAsync(merchantId, true);
        }

        public async Task<IEnumerable<IWalletState>> GetNotExpiredAsync()
        {
            var wallets = (await _walletRepository.GetNotExpired()).ToList();

            var transactions = new List<IBlockchainTransaction>();

            foreach (var batch in wallets.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(x => _blockchainTransactionRepository.GetByWallet(x.Address)
                    .ContinueWith(t =>
                    {
                        lock (transactions)
                        {
                            transactions.AddRange(t.Result);
                        }
                    })));
            }

            return wallets.Select(x => new WalletState
            {
                Address = x.Address,
                DueDate = x.DueDate,
                Transactions = transactions.Where(t => t.WalletAddress == x.Address)
            });
        }
    }
}
