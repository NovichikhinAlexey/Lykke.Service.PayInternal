using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantGroup
{
    public class MerchantGroupRepository : IMerchantGroupRepository
    {
        private readonly INoSQLTableStorage<MerchantGroupEntity> _tableStorage;

        private readonly INoSQLTableStorage<AzureIndex> _groupIndexStorage;

        public MerchantGroupRepository(
            [NotNull] INoSQLTableStorage<MerchantGroupEntity> storage,
            [NotNull] INoSQLTableStorage<AzureIndex> groupIndexStorage)
        {
            _tableStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _groupIndexStorage = groupIndexStorage ?? throw new ArgumentNullException(nameof(groupIndexStorage));
        }

        public async Task<IMerchantGroup> GetAsync(string id)
        {
            AzureIndex index = await _groupIndexStorage.GetDataAsync(
                MerchantGroupEntity.IndexById.GeneratePartitionKey(id),
                MerchantGroupEntity.IndexById.GenerateRowKey());

            if (index == null)
                return null;

            MerchantGroupEntity entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.Groups.MerchantGroup>(entity);
        }

        public async Task UpdateAsync(IMerchantGroup src)
        {
            AzureIndex index = await _groupIndexStorage.GetDataAsync(
                MerchantGroupEntity.IndexById.GeneratePartitionKey(src.Id),
                MerchantGroupEntity.IndexById.GenerateRowKey());

            if (index == null)
                throw new KeyNotFoundException();

            MerchantGroupEntity updatedEntity = await _tableStorage.MergeAsync(
                MerchantGroupEntity.ByOwner.GeneratePartitionKey(index.PrimaryPartitionKey),
                MerchantGroupEntity.ByOwner.GenerateRowKey(index.PrimaryRowKey),
                entity =>
                {
                    if (src.DisplayName != null)
                        entity.DisplayName = src.DisplayName;

                    if (!string.IsNullOrEmpty(src.Merchants))
                        entity.Merchants = src.Merchants;

                    entity.MerchantGroupUse = src.MerchantGroupUse;

                    return entity;
                });

            if (updatedEntity == null)
                throw new KeyNotFoundException();
        }

        public async Task<IMerchantGroup> CreateAsync(IMerchantGroup src)
        {
            MerchantGroupEntity entity = MerchantGroupEntity.ByOwner.Create(src);

            await _tableStorage.InsertThrowConflict(entity);

            AzureIndex index = MerchantGroupEntity.IndexById.Create(entity);

            await _groupIndexStorage.InsertThrowConflict(index);

            return Mapper.Map<Core.Domain.Groups.MerchantGroup>(entity);
        }

        public async Task DeleteAsync(string id)
        {
            AzureIndex index = await _groupIndexStorage.GetDataAsync(
                MerchantGroupEntity.IndexById.GeneratePartitionKey(id),
                MerchantGroupEntity.IndexById.GenerateRowKey());

            if (index == null)
                throw new KeyNotFoundException();

            await _groupIndexStorage.DeleteAsync(index);

            await _tableStorage.DeleteAsync(index.PrimaryPartitionKey, index.PrimaryRowKey);
        }

        public async Task<IReadOnlyList<IMerchantGroup>> GetByOwnerAsync(string ownerId)
        {
            IEnumerable<MerchantGroupEntity> groups =
                await _tableStorage.GetDataAsync(MerchantGroupEntity.ByOwner.GeneratePartitionKey(ownerId));

            return Mapper.Map<IReadOnlyList<Core.Domain.Groups.MerchantGroup>>(groups);
        }
    }
}
