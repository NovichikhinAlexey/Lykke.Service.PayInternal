using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    public class WalletRepository : IWalletRepository
    {
        private readonly INoSQLTableStorage<WalletEntity> _tableStorage;

        public WalletRepository(INoSQLTableStorage<WalletEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task SaveAsync(IWallet wallet)
        {
            var newItem = WalletEntity.ByMerchant.Create(wallet);

            await _tableStorage.InsertOrMergeAsync(newItem);
        }

        public async Task<IEnumerable<IWallet>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IEnumerable<IWallet>> GetByMerchantAsync(string merchantId)
        {
            return await _tableStorage.GetDataAsync(WalletEntity.ByMerchant.GeneratePartitionKey(merchantId));
        }
    }
}
