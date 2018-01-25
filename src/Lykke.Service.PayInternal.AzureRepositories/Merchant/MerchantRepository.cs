using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly INoSQLTableStorage<MerchantEntity> _tableStorage;

        public MerchantRepository(
            INoSQLTableStorage<MerchantEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IMerchant> GetAsync(string id)
        {
            return await _tableStorage.GetDataAsync(
                MerchantEntity.GeneratePartitionKey(),
                MerchantEntity.GenerateRowKey(id));
        }

        public async Task<IEnumerable<IMerchant>> GetAsync()
        {
            return await _tableStorage.GetDataAsync(MerchantEntity.GeneratePartitionKey());
        }

        public async Task<IMerchant> SaveAsync(IMerchant merchant)
        {
            var newItem = MerchantEntity.Create(merchant);

            await _tableStorage.InsertOrMergeAsync(newItem);

            return newItem;
        }
    }
}
