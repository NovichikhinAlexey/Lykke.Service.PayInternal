using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class BitcoinController : Controller
    {
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly IMerchantRepository _merchantRepository;
        private readonly ILog _log;

        public BitcoinController(
            IMerchantWalletsService merchantWalletsService,
            IMerchantRepository merchantRepository,
            ILog log)
        {
            _merchantWalletsService =
                merchantWalletsService ?? throw new ArgumentNullException(nameof(merchantWalletsService));
            _merchantRepository = merchantRepository ?? throw new ArgumentNullException(nameof(merchantRepository));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates new bitcoin address
        /// </summary>
        /// <returns></returns>
        [HttpPost("address/{merchantId}")]
        [SwaggerOperation("CreateBitcoinAddress")]
        [ProducesResponseType(typeof(WalletAddressResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateAddress(CreateWalletRequest request)
        {
            if (request.DueDate <= DateTime.UtcNow)
                return BadRequest(ErrorResponse.Create("DueDate has to be in the future"));

            if (string.IsNullOrEmpty(request.MerchantId))
                return BadRequest(ErrorResponse.Create("MerchantId can't be empty"));

            var merchant = await _merchantRepository.GetAsync(request.MerchantId);
            if (merchant == null)
                return NotFound();

            try
            {
                var address = await _merchantWalletsService.CreateAddress(request.ToDomain());

                return Ok(new WalletAddressResponse {Address = address});
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(BitcoinController), nameof(CreateAddress), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
