using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/files")]
    public class FilesController : Controller
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }
        /// <summary>
        /// Returns a collection of merchant files.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of file info.</returns>
        /// <response code="200">The collection of file info.</response>
        [HttpGet]
        [Route("{merchantId}")]
        [SwaggerOperation("FileGetAll")]
        [ProducesResponseType(typeof(IEnumerable<FileInfoModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllAsync(string merchantId)
        {
            IEnumerable<FileInfo> fileInfos = await _fileService.GetInfoAsync(merchantId);

            return Ok(Mapper.Map<List<FileInfoModel>>(fileInfos));
        }
        /// <summary>
        /// Returns file content.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="fileId">The file id.</param>
        /// <returns>The file stream.</returns>
        /// <response code="200">The file stream.</response>
        /// <response code="404">File info not found.</response>
        [HttpGet]
        [Route("{merchantId}/{fileId}")]
        [SwaggerOperation("FileGetContent")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContentAsync(string merchantId, string fileId)
        {
            FileInfo fileInfo = await _fileService.GetInfoAsync(merchantId, fileId);

            if (fileInfo == null)
                return NotFound();

            byte[] content = await _fileService.GetFileAsync(fileId);

            return Ok(content);
        }

        /// <summary>
        /// Saves file.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="file">The file.</param>
        /// <response code="204">File successfuly uploaded.</response>
        /// <response code="400">Invalid file.</response>
        [HttpPost]
        [Route("{merchantId}")]
        [SwaggerOperation("FileUpload")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UploadAsync(string merchantId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            var fileInfo = new FileInfo
            {
                MerchantId = merchantId,
                Type = file.ContentType,
                Name = file.FileName,
                Size = (int)file.Length
            };

            byte[] content;

            using (var ms = new System.IO.MemoryStream())
            {
                file.CopyTo(ms);
                content = ms.ToArray();
            }

            await _fileService.SaveAsync(fileInfo, content);

            return NoContent();
        }

        /// <summary>
        /// Deletes file.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="fileId">The file id.</param>
        /// <response code="200">File successfully deleted.</response>
        [HttpDelete]
        [Route("{merchantId}/{fileId}")]
        [SwaggerOperation("FileDelete")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteAsync(string merchantId, string fileId)
        {
            await _fileService.DeleteAsync(merchantId, fileId);

            return NoContent();
        }
    }
}
