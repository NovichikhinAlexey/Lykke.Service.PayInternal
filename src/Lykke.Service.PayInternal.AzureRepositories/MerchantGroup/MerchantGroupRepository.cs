using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Groups;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantGroup
{
    public class MerchantGroupRepository : IMerchantGroupRepository
    {
        private readonly INoSQLTableStorage<MerchantGroupEntity> _storage;

        private readonly INoSQLTableStorage<AzureIndex> _groupIndexStorage;

        public MerchantGroupRepository(
            [NotNull] INoSQLTableStorage<MerchantGroupEntity> storage,
            [NotNull] INoSQLTableStorage<AzureIndex> groupIndexStorage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _groupIndexStorage = groupIndexStorage ?? throw new ArgumentNullException(nameof(groupIndexStorage));
        }

        public async Task<IMerchantGroup> GetAsync(string id)
        {
            AzureIndex index = await _groupIndexStorage.GetDataAsync(
                MerchantGroupEntity.IndexById.GeneratePartitionKey(id),
                MerchantGroupEntity.IndexById.GenerateRowKey());

            if (index == null)
                return null;

            MerchantGroupEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.Groups.MerchantGroup>(entity);
        }

        public async Task<IMerchantGroup> CreateAsync(IMerchantGroup src)
        {
            MerchantGroupEntity entity = MerchantGroupEntity.ByOwner.Create(src);

            await _storage.InsertThrowConflict(entity);

            AzureIndex index = MerchantGroupEntity.IndexById.Create(entity);

            await _groupIndexStorage.InsertThrowConflict(index);

            return Mapper.Map<Core.Domain.Groups.MerchantGroup>(entity);
        }
    }
}
