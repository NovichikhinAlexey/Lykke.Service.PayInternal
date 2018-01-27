using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly INoSQLTableStorage<MerchantEntity> _storage;

        public MerchantRepository(
            INoSQLTableStorage<MerchantEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<IMerchant>> GetAsync()
        {
            IEnumerable<MerchantEntity> entities =
                await _storage.GetDataAsync(GetPartitionKey());

            return entities.ToList();
        }
        
        public async Task<IMerchant> GetAsync(string merchantId)
        {
            return await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(merchantId));
        }

        public async Task<IMerchant> InsertAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(), GetRowKey());
            entity.Map(merchant);

            await _storage.InsertAsync(entity);

            return entity;
        }
        
        public async Task ReplaceAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(), GetRowKey(merchant.Id));
            entity.Map(merchant);
            
            entity.ETag = "*";
            
            await _storage.ReplaceAsync(entity);
        }

        public async Task DeleteAsync(string merchantId)
        {
            await _storage.DeleteAsync(GetPartitionKey(), GetRowKey(merchantId));
        }

        private static string GetPartitionKey()
            => "M"; // TODO: Change to 'Merchant'

        private static string GetRowKey(string merchantId)
            => merchantId;

        private static string GetRowKey()
            => Guid.NewGuid().ToString("D");
    }
}
