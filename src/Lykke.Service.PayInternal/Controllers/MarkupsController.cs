using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Markups;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/markups")]
    public class MarkupsController : Controller
    {
        private readonly IMarkupService _markupService;
        private readonly IMerchantService _merchantService;
        private readonly ILog _log;

        public MarkupsController(
            IMarkupService markupService,
            ILog log,
            IMerchantService merchantService)
        {
            _markupService = markupService;
            _log = log;
            _merchantService = merchantService;
        }

        /// <summary>
        /// Returns the default markup values for all asset pairs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("default")]
        [SwaggerOperation("GetDefaults")]
        [ProducesResponseType(typeof(IEnumerable<MarkupResponse>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetDefaults()
        {
            try
            {
                IReadOnlyList<IMarkup> defaults = await _markupService.GetDefaultsAsync();

                return Ok(Mapper.Map<IEnumerable<MarkupResponse>>(defaults));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(GetDefaults), ex);

                throw;
            }
        }

        /// <summary>
        /// Returns the default markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("default/{assetPairId}")]
        [SwaggerOperation("GetDefault")]
        [ProducesResponseType(typeof(MarkupResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDefault(string assetPairId)
        {
            try
            {
                IMarkup markup = await _markupService.GetDefaultAsync(assetPairId);

                if (markup == null) return NotFound(ErrorResponse.Create("Default markup has not been set"));

                return Ok(Mapper.Map<MarkupResponse>(markup));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(GetDefault), ex);

                throw;
            }
        }

        /// <summary>
        /// Updates markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("default/{assetPairId}")]
        [SwaggerOperation("SetDefault")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> SetDefault(string assetPairId, [FromBody] UpdateMarkupRequest request)
        {
            try
            {
                await _markupService.SetDefaultAsync(assetPairId, request.PriceAssetPairId, request.PriceMethod, request);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(SetDefault), ex);

                throw;
            }
        }

        /// <summary>
        /// Returns markup values for merchant
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchant/{merchantId}")]
        [SwaggerOperation("GetAllForMerchant")]
        [ProducesResponseType(typeof(IEnumerable<MarkupResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAllForMerchant(string merchantId)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            try
            {
                IReadOnlyList<IMarkup> markups = await _markupService.GetForMerchantAsync(merchantId);

                return Ok(Mapper.Map<IEnumerable<MarkupResponse>>(markups));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(GetAllForMerchant), ex);

                throw;
            }
        }

        /// <summary>
        /// Returns markup value for merchant and asset pair id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchant/{merchantId}/{assetPairId}")]
        [SwaggerOperation("GetForMerchant")]
        [ProducesResponseType(typeof(MarkupResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetForMerchant(string merchantId, string assetPairId)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            try
            {
                IMarkup markup = await _markupService.GetForMerchantAsync(merchantId, assetPairId);

                if (markup == null) return NotFound(ErrorResponse.Create("Markup has not been set"));

                return Ok(Mapper.Map<MarkupResponse>(markup));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(GetForMerchant), ex);

                throw;
            }
        }

        /// <summary>
        /// Updates markup values for merchant and asset pair
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("merchant/{merchantId}/{assetPairId}")]
        [SwaggerOperation("SetForMerchant")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetForMerchant(string merchantId, string assetPairId,
            [FromBody] UpdateMarkupRequest request)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            try
            {
                await _markupService.SetForMerchantAsync(assetPairId, merchantId, request.PriceAssetPairId, request.PriceMethod, request);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(MarkupsController), nameof(SetForMerchant), ex);

                throw;
            }
        }
    }
}
