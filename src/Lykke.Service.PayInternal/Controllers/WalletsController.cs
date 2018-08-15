using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Wallets;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IBcnWalletUsageService _bcnWalletUsageService;
        private readonly IWalletManager _walletManager;
        private readonly IBlockchainAddressValidator _blockchainAddressValidator;

        public WalletsController(
            IBcnWalletUsageService bcnWalletUsageService, 
            IWalletManager walletManager, 
            IBlockchainAddressValidator blockchainAddressValidator)
        {
            _bcnWalletUsageService = bcnWalletUsageService;
            _walletManager = walletManager;
            _blockchainAddressValidator = blockchainAddressValidator;
        }

        /// <summary>
        /// Notifies about wallet expiration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("expired")]
        [SwaggerOperation("SetExpired")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> SetExpired([FromBody] BlockchainWalletExpiredRequest request)
        {
            bool isValid;

            try
            {
                isValid = await _blockchainAddressValidator.Execute(request.WalletAddress, request.Blockchain);
            }
            catch (BlockchainTypeNotSupported)
            {
                isValid = true;
            }

            if (!isValid)
                return BadRequest(ErrorResponse.Create("Wallet address is not valid"));

            bool released = await _bcnWalletUsageService.ReleaseAsync(request.WalletAddress, request.Blockchain);

            if (released)
                return Ok();

            return BadRequest(ErrorResponse.Create("Couldn't set wallet as expired"));
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
            IEnumerable<IWalletState> wallets = await _walletManager.GetNotExpiredStateAsync();

            return Ok(Mapper.Map<IEnumerable<WalletStateResponse>>(wallets));
        }
    }
}
