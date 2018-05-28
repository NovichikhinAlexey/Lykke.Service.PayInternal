using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileInfoRepository _fileInfoRepository;

        public FileService(
            IFileInfoRepository fileInfoRepository,
            IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
            _fileInfoRepository = fileInfoRepository;
        }
        public async Task<IEnumerable<FileInfo>> GetInfoAsync(string invoiceId)
        {
            return await _fileInfoRepository.GetAsync(invoiceId);
        }
        public async Task<FileInfo> GetInfoAsync(string merchantId, string fileId)
        {
            return await _fileInfoRepository.GetAsync(merchantId, fileId);
        }
        public async Task<byte[]> GetFileAsync(string fileId)
        {
            return await _fileRepository.GetAsync(fileId);
        }

        public async Task SaveAsync(FileInfo fileInfo, byte[] content)
        {
            string fileId = await _fileInfoRepository.InsertAsync(fileInfo);
            await _fileRepository.InsertAsync(content, fileId);
        }

        public async Task DeleteAsync(string merchantId, string fileId)
        {
            await _fileRepository.DeleteAsync(fileId);
        }
    }
}
