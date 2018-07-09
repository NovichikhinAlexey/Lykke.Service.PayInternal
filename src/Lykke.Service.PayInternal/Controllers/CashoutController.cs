using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Cashout;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInternal.Models.Cashout;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/cashout/[action]")]
    public class CashoutController : Controller
    {
        private readonly ICashoutService _cashoutService;
        private readonly ILog _log;

        public CashoutController(
            [NotNull] ICashoutService cashoutService, 
            [NotNull] ILog log)
        {
            _cashoutService = cashoutService ?? throw new ArgumentNullException(nameof(cashoutService));
            _log = log.CreateComponentScope(nameof(CashoutController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Executes cashout operation
        /// </summary>
        /// <param name="request">Cashout request details</param>
        /// <returns></returns>
        /// <response code="200">Cashout result details</response>
        /// <response code="400">Asset network not defined, insufficient funds, cashout failed,
        /// default merchant wallet not found, merchant is not the owner of the wallet, 
        /// cashout hotwallet not defined</response>
        [HttpPost]
        [SwaggerOperation(nameof(Execute))]
        [ProducesResponseType(typeof(CashoutResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] CashoutModel request)
        {
            try
            {
                CashoutResult cashoutResult = await _cashoutService.ExecuteAsync(Mapper.Map<CashoutCommand>(request));

                return Ok(Mapper.Map<CashoutResponse>(cashoutResult));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.WriteError(nameof(Execute), new {e.AssetId}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InvalidOperationException e)
            {
                _log.WriteError(nameof(Execute), request, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InsufficientFundsException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.WalletAddress,
                    e.AssetId
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (CashoutOperationFailedException e)
            {
                _log.WriteError(nameof(Execute), new {errors = e.TransferErrors}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.AssetId,
                    e.MerchantId,
                    e.PaymentDirection
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.AssetId,
                    e.MerchantId,
                    e.PaymentDirection
                });

                return BadRequest(ErrorResponse.Create(e.MerchantId));
            }
            catch (MerchantWalletOwnershipException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.MerchantId,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (CashoutHotwalletNotDefinedException e)
            {
                _log.WriteError(nameof(Execute), new {e.Blockchain}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
