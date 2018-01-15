using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Microsoft.WindowsAzure.Storage.Table;

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
            var itemByMerchant = WalletEntity.ByMerchant.Create(wallet);

            var itemByDueDate = WalletEntity.ByDueDate.Create(wallet);

            await _tableStorage.InsertAsync(itemByMerchant);

            await _tableStorage.InsertAsync(itemByDueDate);
        }

        public async Task<IEnumerable<IWallet>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();

        }

        public async Task<IWallet> GetAsync(string merchantId, string address)
        {
            return await _tableStorage.GetDataAsync(
                WalletEntity.ByMerchant.GeneratePartitionKey(merchantId),
                WalletEntity.ByMerchant.GenerateRowKey(address));
        }

        public async Task<IEnumerable<IWallet>> GetByMerchantAsync(string merchantId, bool nonEmptyOnly = false)
        {
            if (nonEmptyOnly)
            {
                var filter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                        WalletEntity.ByMerchant.GeneratePartitionKey(merchantId)),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDouble("Amount", QueryComparisons.GreaterThan, 0d));

                var query = new TableQuery<WalletEntity>().Where(filter);

                return await _tableStorage.WhereAsync(query);
            }
           
            return await _tableStorage.GetDataAsync(WalletEntity.ByMerchant.GeneratePartitionKey(merchantId));
        }
    }
}
