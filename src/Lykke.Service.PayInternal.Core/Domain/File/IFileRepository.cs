using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.File
{
    public interface IFileRepository
    {
        Task<byte[]> GetAsync(string id);

        Task<string> GetBlobUrl(string fileName);

        Task<string> InsertAsync(byte[] file, string id);

        Task DeleteAsync(string id);
    }
}
