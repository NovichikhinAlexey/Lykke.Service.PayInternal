using ImageSharp;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Services
{
    public class FileService : IFileService
    {
        private const string PngContentType = "image/png";
        private const string JpegContentType = "image/jpeg";
        private static readonly string[] ImageContentTypes = { PngContentType, JpegContentType };

        private readonly IFileRepository _fileRepository;
        private readonly MerchantSettings _merchantSettings;
        private readonly IFileInfoRepository _fileInfoRepository;

        public FileService(
            MerchantSettings merchantSettings,
            IFileInfoRepository fileInfoRepository,
            IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
            _merchantSettings = merchantSettings;
            _fileInfoRepository = fileInfoRepository;
        }
        public async Task<IEnumerable<FileInfo>> GetInfoAsync(string fileId)
        {
            return await _fileInfoRepository.GetAsync(fileId);
        }
        public async Task<FileInfo> GetInfoAsync(string merchantId, string fileId)
        {
            return await _fileInfoRepository.GetAsync(merchantId, fileId);
        }
        public async Task<byte[]> GetFileAsync(string fileId)
        {
            return await _fileRepository.GetAsync(fileId);
        }

        public async Task<string> GetMerchantLogoUrl(string merchantId)
        {
            var files = await _fileInfoRepository.GetAsync(merchantId);
            var file = files?.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            if (file == null)
                throw new MerchantLogoNotFoundException(merchantId);

            string blobUrl = await _fileRepository.GetBlobUrl(GetMerchantLogoFileName(file.Id, merchantId, file.Type));

            if (string.IsNullOrEmpty(blobUrl))
                throw new MerchantLogoNotFoundException(merchantId);

            return blobUrl;
        }

        public async Task SaveAsync(string merchantId, IFormFile file)
        {
            byte[] content;
            byte[] merchantLogoBytes = new byte[] { };
            bool isCreateMerchantLogo = ImageContentTypes.Contains(file.ContentType);

            using (var ms = new System.IO.MemoryStream())
            {
                file.CopyTo(ms);
                content = ms.ToArray();

                if (isCreateMerchantLogo)
                {
                    var img = new Image(ms);

                    var result = img.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(_merchantSettings.LogoSize, _merchantSettings.LogoSize * img.Width / img.Height)
                    });

                    using (var outStream = new System.IO.MemoryStream())
                    {
                        if (file.ContentType == PngContentType)
                        {
                            result.SaveAsPng(outStream);
                        }
                        else
                        {
                            result.SaveAsJpeg(outStream);
                        }
                        
                        merchantLogoBytes = outStream.ToArray();
                    }
                }
            }

            var fileInfo = new FileInfo
            {
                MerchantId = merchantId,
                Type = file.ContentType,
                Name = file.FileName,
                Size = (int)file.Length
            };

            string fileId = await _fileInfoRepository.InsertAsync(fileInfo);
            await _fileRepository.InsertAsync(content, fileId);

            if (isCreateMerchantLogo)
            {
                await _fileRepository.InsertAsync(merchantLogoBytes, GetMerchantLogoFileName(fileId, merchantId, file.ContentType));
            }
        }

        public async Task DeleteAsync(string merchantId, string fileId)
        {
            await _fileRepository.DeleteAsync(fileId);
            await _fileInfoRepository.DeleteAsync(merchantId, fileId);
        }

        private string GetMerchantLogoFileName(string fileId, string merchantId, string contentType)
        {
            string extension;

            switch (contentType)
            {
                case PngContentType:
                    extension = ".png";
                    break;
                default:
                    extension = ".jpeg";
                    break;
            }

            return $"{fileId}_{merchantId}_logo_{_merchantSettings.LogoSize}{extension}";
        }
    }
}
