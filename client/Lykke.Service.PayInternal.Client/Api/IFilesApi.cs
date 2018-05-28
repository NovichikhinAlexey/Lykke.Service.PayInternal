using Lykke.Service.PayInternal.Client.Models.File;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IFilesApi
    {
        [Get("/api/files/{merchantId}")]
        Task<IReadOnlyList<FileInfoModel>> GetAllAsync(string merchantId);
        [Get("/api/files/{merchantId}/{fileId}")]
        Task<HttpResponseMessage> GetAsync(string merchantId, string fileId);

        [Multipart]
        [Post("/api/files/{merchantId}")]
        Task UploadAsync(string merchantId, [AliasAs("file")] StreamPart stream);

        [Delete("/api/files/{merchantId}/{fileId}")]
        Task<HttpResponseMessage> DeleteAsync(string merchantId, string fileId);
    }
}
