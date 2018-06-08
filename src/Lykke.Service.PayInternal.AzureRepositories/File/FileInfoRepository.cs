using AutoMapper;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.File;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.File
{
    public class FileInfoRepository : IFileInfoRepository
    {
        private readonly INoSQLTableStorage<FileInfoEntity> _storage;

        public FileInfoRepository(INoSQLTableStorage<FileInfoEntity> storage)
        {
            _storage = storage;
        }
        public async Task<IReadOnlyList<FileInfo>> GetAsync(string merchantId)
        {
            IEnumerable<FileInfoEntity> entities = await _storage.GetDataAsync(GetPartitionKey(merchantId));

            return Mapper.Map<List<FileInfo>>(entities);
        }
        public async Task<FileInfo> GetAsync(string merchantId, string fileId)
        {
            FileInfoEntity entity = await _storage.GetDataAsync(GetPartitionKey(merchantId), fileId);

            return Mapper.Map<FileInfo>(entity);
        }

        public async Task<string> InsertAsync(FileInfo fileInfo)
        {
            var entity = new FileInfoEntity(GetPartitionKey(fileInfo.MerchantId), GetRowKey());

            Mapper.Map(fileInfo, entity);

            await _storage.InsertAsync(entity);

            return entity.RowKey;
        }

        public async Task UpdateAsync(FileInfo fileInfo)
        {
            var entity = new FileInfoEntity(GetPartitionKey(fileInfo.MerchantId), fileInfo.Id);

            Mapper.Map(fileInfo, entity);

            await _storage.ReplaceAsync(entity);
        }

        public async Task DeleteAsync(string merchantId)
        {
            IEnumerable<FileInfoEntity> entities = await _storage.GetDataAsync(GetPartitionKey(merchantId));

            await _storage.DeleteAsync(entities);
        }

        public async Task DeleteAsync(string merchantId, string fileId)
        {
            await _storage.DeleteAsync(GetPartitionKey(merchantId), fileId);
        }

        private static string GetPartitionKey(string merchantId)
            => merchantId;

        private static string GetRowKey()
            => Guid.NewGuid().ToString("D");
    }
}
