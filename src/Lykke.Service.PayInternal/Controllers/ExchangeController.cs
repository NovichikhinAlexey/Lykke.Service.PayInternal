using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Exchange;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Exchange;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/exchange")]
    public class ExchangeController : Controller
    {
        private readonly IExchangeService _exchangeService;
        private readonly ILog _log;

        public ExchangeController(
            [NotNull] ILog log, 
            [NotNull] IExchangeService exchangeService)
        {
            _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
            _log = log.CreateComponentScope(nameof(ExchangeController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Exchanges assets
        /// </summary>
        /// <param name="request">Exchange operation details</param>
        /// <returns></returns>
        /// <response code="200">Exchange result</response>
        /// <response code="400">Asset network not defined, operation not supported, asset pair not found, insufficient funds, exchange failed</response>
        [HttpPost]
        [SwaggerOperation(nameof(Execute))]
        [ProducesResponseType(typeof(ExchangeResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] ExchangeModel request)
        {
            try
            {
                ExchangeResult exchangeResult =
                    await _exchangeService.ExecuteAsync(Mapper.Map<ExchangeCommand>(request));

                return Ok(Mapper.Map<ExchangeResponse>(exchangeResult));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.WriteError(nameof(Execute), new {e.AssetId}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (ExchangeOperationNotSupportedException e)
            {
                _log.WriteError(nameof(Execute), request, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InvalidOperationException e)
            {
                _log.WriteError(nameof(Execute), request, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (AssetPairUnknownException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.BaseAssetId,
                    e.QuotingAssetId
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (ExchangeOperationInsufficientFundsException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.WalletAddress,
                    e.AssetId
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (ExchangeOperationFailedException e)
            {
                _log.WriteError(nameof(Execute), new {errors = e.TransferErrors}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
