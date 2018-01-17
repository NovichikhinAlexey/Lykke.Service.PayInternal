using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class MerchantsController : Controller
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly ILog _log;

        public MerchantsController(
            IMerchantRepository merchantRepository,
            ILog log)
        {
            _merchantRepository = merchantRepository ?? throw new ArgumentNullException(nameof(merchantRepository));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [HttpPost]
        [SwaggerOperation("Create")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Create([FromBody] CreateMerchantRequest request)
        {
            try
            {
                await _merchantRepository.SaveAsync(request.GetMerchant());

                return Ok();
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(MerchantsController), nameof(Create), request.ToJson(), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpPost("{id}/publicKey")]
        [SwaggerOperation("UpdatePublicKey")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdatePublicKey(IFormFile file, string id)
        {
            var merchant = await _merchantRepository.GetAsync(id);
            if (merchant == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(ErrorResponse.Create("Empty file"));
            }

            try
            {
                var fileContent = await file.OpenReadStream().ToBytesAsync();

                merchant.PublicKey = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

                await _merchantRepository.SaveAsync(merchant);

                return NoContent();
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(MerchantsController), nameof(UpdatePublicKey), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
