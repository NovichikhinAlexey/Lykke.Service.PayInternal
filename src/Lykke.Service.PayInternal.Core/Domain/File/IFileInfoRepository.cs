using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.File
{
    public interface IFileInfoRepository
    {
        Task<IReadOnlyList<FileInfo>> GetAsync(string merchantId);
        Task<FileInfo> GetAsync(string merchantId, string fileId);

        Task<string> InsertAsync(FileInfo fileInfo);

        Task UpdateAsync(FileInfo fileInfo);

        Task DeleteAsync(string merchantId, string fileId);
    }
}
