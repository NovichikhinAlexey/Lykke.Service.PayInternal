using Lykke.Service.PayInternal.Core.Domain.File;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FileInfo>> GetInfoAsync(string merchantId);
        Task<FileInfo> GetInfoAsync(string merchantId, string fileId);
        Task<byte[]> GetFileAsync(string fileId);

        Task SaveAsync(FileInfo fileInfo, byte[] content);

        Task DeleteAsync(string merchantId, string fileId);
    }
}
