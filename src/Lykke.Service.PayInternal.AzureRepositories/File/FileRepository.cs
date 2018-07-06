using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.File;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.File
{
    public class FileRepository : IFileRepository
    {
        private readonly IBlobStorage _storage;

        private const string ContainerName = "merchantfiles";

        public FileRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task<byte[]> GetAsync(string id)
        {
            using (var stream = await _storage.GetAsync(ContainerName, id))
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public async Task<string> GetBlobUrl(string fileName)
        {
            return await _storage.HasBlobAsync(ContainerName, fileName) ? _storage.GetBlobUrl(ContainerName, fileName) : null;
        }

        public async Task<string> InsertAsync(byte[] file, string id)
        {
            await _storage.SaveBlobAsync(ContainerName, id, file);
            return _storage.GetBlobUrl(ContainerName, id);
        }

        public async Task DeleteAsync(string id)
        {
            await _storage.DelBlobAsync(ContainerName, id);
        }
    }
}
