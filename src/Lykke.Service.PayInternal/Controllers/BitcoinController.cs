using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class BitcoinController : Controller
    {
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly ILog _log;

        public BitcoinController(
            IMerchantWalletsService merchantWalletsService,
            ILog log)
        {
            _merchantWalletsService =
                merchantWalletsService ?? throw new ArgumentNullException(nameof(merchantWalletsService));
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
        public async Task<IActionResult> CreateAddress(string merchantId)
        {
            try
            {
                var address = await _merchantWalletsService.CreateAddress(merchantId);

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
