using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    public class 
        VirtualWalletRepository : IVirtualWalletRepository
    {
        private readonly INoSQLTableStorage<VirtualWalletEntity> _tableStorage;
        private readonly INoSQLTableStorage<AzureIndex> _walletIdIndexStorage;
        private readonly INoSQLTableStorage<AzureIndex> _dueDateIndexStorage;

        public VirtualWalletRepository(
            INoSQLTableStorage<VirtualWalletEntity> tableStorage,
            INoSQLTableStorage<AzureIndex> walletIdIndexStorage, 
            INoSQLTableStorage<AzureIndex> dueDateIndexStorage)
        {
            _tableStorage = tableStorage;
            _walletIdIndexStorage = walletIdIndexStorage;
            _dueDateIndexStorage = dueDateIndexStorage;
        }

        public async Task<IVirtualWallet> CreateAsync(IVirtualWallet wallet)
        {
            VirtualWalletEntity entity = VirtualWalletEntity.ByMerchantId.Create(wallet);

            await _tableStorage.InsertAsync(entity);

            AzureIndex indexByWalletId = VirtualWalletEntity.IndexByWalletId.Create(entity);

            await _walletIdIndexStorage.InsertAsync(indexByWalletId);

            AzureIndex indexByDueDate = VirtualWalletEntity.IndexByDueDate.Create(entity);

            await _dueDateIndexStorage.InsertAsync(indexByDueDate);

            return Mapper.Map<VirtualWallet>(entity);
        }

        public async Task<IVirtualWallet> GetAsync(string merchantId, string walletId)
        {
            VirtualWalletEntity entity = await _tableStorage.GetDataAsync(
                VirtualWalletEntity.ByMerchantId.GeneratePartitionKey(merchantId),
                VirtualWalletEntity.ByMerchantId.GenerateRowKey(walletId));

            return Mapper.Map<VirtualWallet>(entity);
        }

        public async Task<IVirtualWallet> FindAsync(string walletId)
        {
            AzureIndex index = await _walletIdIndexStorage.GetDataAsync(
                VirtualWalletEntity.IndexByWalletId.GeneratePartitionKey(walletId),
                VirtualWalletEntity.IndexByWalletId.GenerateRowKey());

            if (index == null) return null;

            VirtualWalletEntity entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<VirtualWallet>(entity);
        }

        public async Task SaveAsync(IVirtualWallet wallet)
        {
            string partitionKey = VirtualWalletEntity.ByMerchantId.GeneratePartitionKey(wallet.MerchantId);

            string rowKey = VirtualWalletEntity.ByMerchantId.GenerateRowKey(wallet.Id);

            VirtualWalletEntity exItem =
                wallet.Id != null ? await _tableStorage.GetDataAsync(partitionKey, rowKey) : null;

            if (exItem != null)
            {
                await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
                {
                    entity.BlockchainWallets = wallet.BlockchainWallets;

                    return entity;
                });

                return;
            }

            VirtualWalletEntity newEntity = VirtualWalletEntity.ByMerchantId.Create(wallet);

            await _tableStorage.InsertAsync(newEntity);
        }

        public async Task<IReadOnlyList<IVirtualWallet>> GetByDueDateAsync(DateTime dueDateGreaterThan)
        {
            string gtDate = VirtualWalletEntity.IndexByDueDate.GeneratePartitionKey(dueDateGreaterThan);

            // fake date to fit partitionKey format and not to get data from other partitions 
            string ltDate = VirtualWalletEntity.IndexByDueDate.GeneratePartitionKey(dueDateGreaterThan.AddYears(100));

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, gtDate),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, ltDate));

            var query = new TableQuery<AzureIndex>().Where(filter);

            IEnumerable<AzureIndex> indecies = await _dueDateIndexStorage.WhereAsync(query);

            IEnumerable<VirtualWalletEntity> entities = await _tableStorage.GetDataAsync(indecies);

            return Mapper.Map<IEnumerable<VirtualWallet>>(entities).ToList();
        }
    }
}
