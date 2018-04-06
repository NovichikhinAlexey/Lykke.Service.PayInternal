using System;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    public class BcnWalletUsageRepository : IBcnWalletUsageRepository
    {
        private readonly INoSQLTableStorage<BcnWalletUsageEntity> _tableStorage;

        public BcnWalletUsageRepository(INoSQLTableStorage<BcnWalletUsageEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IBcnWalletUsage> CreateAsync(IBcnWalletUsage usage)
        {
            var entity = BcnWalletUsageEntity.ByWalletAddress.Create(usage);

            await _tableStorage.InsertAsync(entity);

            return Mapper.Map<BcnWalletUsage>(entity);
        }

        public async Task<IBcnWalletUsage> GetAsync(string walletAddress, BlockchainType blockchain)
        {
            string partitionKey = BcnWalletUsageEntity.ByWalletAddress.GeneratePartitionKey(walletAddress);

            string rowKey = BcnWalletUsageEntity.ByWalletAddress.GenerateRowKey(blockchain);

            BcnWalletUsageEntity entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            return Mapper.Map<BcnWalletUsage>(entity);
        }

        public async Task<bool> TryLockAsync(IBcnWalletUsage usage)
        {
            string partitionKey = BcnWalletUsageEntity.ByWalletAddress.GeneratePartitionKey(usage.WalletAddress);

            string rowKey = BcnWalletUsageEntity.ByWalletAddress.GenerateRowKey(usage.Blockchain);

            return await _tableStorage.InsertOrModifyAsync(partitionKey, rowKey,
                () => BcnWalletUsageEntity.ByWalletAddress.Create(usage),
                existing =>
                {
                    if (!string.IsNullOrEmpty(existing.OccupiedBy))
                    {
                        return false;
                    }

                    existing.OccupiedBy = usage.OccupiedBy;
                    existing.Since = usage.Since;

                    return true;
                });
        }

        public async Task<IBcnWalletUsage> ReleaseAsync(string walletAddress, BlockchainType blockchain)
        {
            string partitionKey = BcnWalletUsageEntity.ByWalletAddress.GeneratePartitionKey(walletAddress);

            string rowKey = BcnWalletUsageEntity.ByWalletAddress.GenerateRowKey(blockchain);

            BcnWalletUsageEntity usage = await _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.OccupiedBy = null;
                entity.Since = DateTime.UtcNow;

                return entity;
            });

            return Mapper.Map<BcnWalletUsage>(usage);
        }
    }
}
