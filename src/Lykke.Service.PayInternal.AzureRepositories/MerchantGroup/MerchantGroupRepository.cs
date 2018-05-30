using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.MerchantGroup;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantGroup
{
    public class MerchantGroupRepository : IMerchantGroupRepository
    {
        private readonly INoSQLTableStorage<MerchantGroupEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _groupIndexStorage;
        public MerchantGroupRepository(
            INoSQLTableStorage<MerchantGroupEntity> storage,
            INoSQLTableStorage<AzureIndex> groupIndexStorage
            )
        {
            _storage = storage;
            _groupIndexStorage = groupIndexStorage;
        }
        public async Task<IMerchantGroup> GetAsync(string ownerId)
        {
            AzureIndex index = await _groupIndexStorage.GetDataAsync(
                MerchantGroupEntity.ByOwner.GeneratePartitionKey(ownerId),
                MerchantGroupEntity.ByOwner.GenerateRowKey());

            if (index == null)
                return null;

            return await _storage.GetDataAsync(index);
        }
        public async Task<IMerchantGroup> InsertAsync(IMerchantGroup merchantGroup)
        {
            var entity = new MerchantGroupEntity(MerchantGroupEntity.GeneratePartitionKey(merchantGroup.OwnerId),
                MerchantGroupEntity.GenerateRowKey());
            Mapper.Map(merchantGroup, entity);
            await _storage.InsertAsync(entity);
            var index = MerchantGroupEntity.ByOwner.Create(entity);
            await _groupIndexStorage.InsertAsync(index);

            return entity;
        }
    }
}
