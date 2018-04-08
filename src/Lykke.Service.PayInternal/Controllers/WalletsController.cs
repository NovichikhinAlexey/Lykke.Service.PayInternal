using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Wallets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IBcnWalletUsageService _bcnWalletUsageService;
        private readonly IWalletManager _walletManager;
        private readonly ILog _log;

        public WalletsController(
            IBcnWalletUsageService bcnWalletUsageService, 
            ILog log, 
            IWalletManager walletManager)
        {
            _bcnWalletUsageService = bcnWalletUsageService;
            _log = log;
            _walletManager = walletManager;
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
        public async Task<IActionResult> SetExpired([FromBody] BlockchainWalletExpiredRequest request)
        {
            try
            {
                await _bcnWalletUsageService.ReleaseAsync(request.WalletAddress, request.Blockchain);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(WalletsController), nameof(SetExpired), request.ToJson(), ex);

                throw;
            }
        }

        /// <summary>
        /// Gets list of wallets with DueDate in the future
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("notExpired")]
        [SwaggerOperation("GetNotExpiredWallets")]
        [ProducesResponseType(typeof(IEnumerable<WalletStateResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetNotExpiredWallets()
        {
            try
            {
                IEnumerable<IWalletState> wallets = await _walletManager.GetNotExpiredStateAsync();

                return Ok(Mapper.Map<IEnumerable<WalletStateResponse>>(wallets));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(WalletsController), nameof(GetNotExpiredWallets), ex);

                throw;
            }
        }
    }
}
