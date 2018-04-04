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

        public async Task<IEnumerable<IWallet>> GetNotExpired()
        {
            var gtDate = WalletEntity.ByDueDate.GeneratePartitionKey(DateTime.UtcNow);

            // fake date to fit partitionKey format and not to get data from other partitions 
            var ltDate = WalletEntity.ByDueDate.GeneratePartitionKey(DateTime.UtcNow.AddYears(100));

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, gtDate),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, ltDate));

            var query = new TableQuery<WalletEntity>().Where(filter);

            return await _tableStorage.WhereAsync(query);
        }
    }
}
