using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Wallets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IBcnWalletUsageService _bcnWalletUsageService;
        private readonly ILog _log;

        public WalletsController(IBcnWalletUsageService bcnWalletUsageService, ILog log)
        {
            _bcnWalletUsageService = bcnWalletUsageService;
            _log = log;
        }

        /// <summary>
        /// Notifies about wallet expiration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("expired")]
        [SwaggerOperation("SetExpired")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> Expired([FromBody] BlockchainWalletExpiredRequest request)
        {
            try
            {
                await _bcnWalletUsageService.ReleaseAsync(request.WalletAddress, request.Blockchain);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(WalletsController), nameof(Expired), request.ToJson(), ex);

                throw;
            }
        }
    }
}
