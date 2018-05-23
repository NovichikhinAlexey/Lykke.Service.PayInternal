using AutoMapper;
using AzureStorage;
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
        public MerchantGroupRepository(
            INoSQLTableStorage<MerchantGroupEntity> storage)
        {
            _storage = storage;
        }
        public async Task<IMerchantGroup> GetAsync(string ownerMerchantId, string groupId)
        {
            return await _storage.GetDataAsync(MerchantGroupEntity.ByOwner.GeneratePartitionKey(ownerMerchantId), groupId);
        }
        public async Task<IMerchantGroup> InsertAsync(IMerchantGroup merchantGroup)
        {
            var entity = new MerchantGroupEntity(MerchantGroupEntity.ByOwner.GeneratePartitionKey(merchantGroup.OwnerMerchantId), MerchantGroupEntity.ByOwner.GenerateRowKey());
            try
            {
                Mapper.Map(merchantGroup, entity);
                await _storage.InsertAsync(entity);
            }
            catch (StorageException ex)
            {
                throw;
            }

            return entity;
        }
    }
}
