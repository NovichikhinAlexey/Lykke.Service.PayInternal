using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Microsoft.WindowsAzure.Storage;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly INoSQLTableStorage<MerchantEntity> _storage;

        private const string ConflictMessage = "Conflict";

        private const int PartitionKeyLength = 3;

        public MerchantRepository(
            INoSQLTableStorage<MerchantEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<IMerchant>> GetAsync()
        {
            IEnumerable<MerchantEntity> entities = await _storage.GetDataAsync();

            return entities.ToList();
        }
        
        public async Task<IMerchant> GetAsync(string merchantName)
        {
            return await _storage.GetDataAsync(GetPartitionKey(merchantName), GetRowKey(merchantName));
        }

        public async Task<IMerchant> InsertAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(merchant.Name), GetRowKey(merchant.Name));

            Mapper.Map(merchant, entity);

            try
            {
                await _storage.InsertAsync(entity);
            }
            catch (StorageException ex)
            {
                if (ex.Message == ConflictMessage)
                    throw new DuplicateMerchantNameException(merchant.Name);

                throw;
            }

            return entity;
        }
        
        public async Task ReplaceAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(merchant.Name), GetRowKey(merchant.Name));

            Mapper.Map(merchant, entity);
            
            entity.ETag = "*";
            
            await _storage.ReplaceAsync(entity);
        }

        public async Task DeleteAsync(string merchantName)
        {
            await _storage.DeleteAsync(GetPartitionKey(merchantName), GetRowKey(merchantName));
        }

        private static string GetPartitionKey(string merchantName)
        {
            string hash = Convert.ToBase64String(SHA1.Create().ComputeHash(merchantName.ToUtf8Bytes()));

            return new string(hash.Take(PartitionKeyLength).ToArray());
        }

        private static string GetRowKey(string merchantName)
            => merchantName;
    }
}
