using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Exchange;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Exchange;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/exchange/[action]")]
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
            catch (InsufficientFundsException e)
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
            catch (ExchangeRateChangedException e)
            {
                _log.WriteError(nameof(Execute), new
                {
                    e.CurrentRate,
                    request.ExpectedRate
                }, e);

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
        }

        /// <summary>
        /// Returns current exchange rate
        /// </summary>
        /// <param name="request">PreExchange operation details</param>
        /// <returns></returns>
        /// <response code="200">Exchange result</response>
        /// <response code="400">Asset network not defined, operation not supported, asset pair not found, insufficient funds, exchange failed</response>
        [HttpPost]
        [SwaggerOperation(nameof(PreExchange))]
        [ProducesResponseType(typeof(ExchangeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> PreExchange([FromBody] PreExchangeModel request)
        {
            try
            {
                ExchangeResult exchangeResult =
                    await _exchangeService.PreExchangeAsync(Mapper.Map<PreExchangeCommand>(request));

                return Ok(Mapper.Map<ExchangeResponse>(exchangeResult));
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
        }
    }
}
