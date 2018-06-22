using Lykke.Service.PayInternal.Core.Domain.File;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FileInfo>> GetInfoAsync(string merchantId);
        Task<FileInfo> GetInfoAsync(string merchantId, string fileId);
        Task<byte[]> GetFileAsync(string fileId);
        Task<string> GetMerchantLogoUrl(string merchantId);

        Task SaveAsync(string merchantId, IFormFile file);

        Task DeleteAsync(string merchantId, string fileId);
    }
}
