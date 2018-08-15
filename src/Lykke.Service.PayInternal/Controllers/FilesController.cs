using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using LykkePay.Common.Validation;

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
        [ProducesResponseType(typeof(IEnumerable<FileInfoModel>), (int) HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> GetAllAsync([Required, RowKey] string merchantId)
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
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> GetContentAsync(
            [Required, RowKey] string merchantId,
            [Required, RowKey] string fileId)
        {
            FileInfo fileInfo = await _fileService.GetInfoAsync(merchantId, fileId);

            if (fileInfo == null)
                return NotFound();

            byte[] content = await _fileService.GetFileAsync(fileId);

            return Ok(content);
        }

        /// <summary>
        /// Get merchant logo blob url
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        /// <response code="200">Merchant logo blob url</response>
        /// <response code="404">Not found</response>
        [HttpGet]
        [Route("logo/{merchantId}")]
        [SwaggerOperation(nameof(GetMerchantLogoUrl))]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetMerchantLogoUrl([Required, RowKey] string merchantId)
        {
            try
            {
                return Ok(await _fileService.GetMerchantLogoUrl(merchantId));
            }
            catch (MerchantLogoNotFoundException)
            {
                return NotFound();
            }
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
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> UploadAsync([Required, RowKey] string merchantId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            await _fileService.SaveAsync(merchantId, file);

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
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> DeleteAsync(
            [Required, RowKey] string merchantId, 
            [Required] string fileId)
        {
            try
            {
                await _fileService.DeleteAsync(merchantId, fileId);
            }
            catch (Core.Exceptions.KeyNotFoundException)
            {
                return NotFound(ErrorResponse.Create("File not found"));
            }

            return NoContent();
        }
    }
}
