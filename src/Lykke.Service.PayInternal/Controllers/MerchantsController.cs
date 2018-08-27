using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Assets;
using Lykke.Service.PayInternal.Models.Markups;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class MerchantsController : Controller
    {
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IMarkupService _markupService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public MerchantsController(
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] ILogFactory logFactory,
            [NotNull] IMarkupService markupService,
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _log = logFactory.CreateLog(this);
            _markupService = markupService ?? throw new ArgumentNullException(nameof(markupService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        /// <summary>
        /// Returns markup values for merchant and asset pair according to merchant's and default's settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchants/{merchantId}/markups/{assetPairId}")]
        [SwaggerOperation("ResolveMarkupByMerchant")]
        [ProducesResponseType(typeof(MarkupResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolveMarkupByMerchant(string merchantId, string assetPairId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);
            assetPairId = Uri.UnescapeDataString(assetPairId);

            try
            {
                IMarkup markup = await _markupService.ResolveAsync(merchantId, assetPairId);

                return Ok(Mapper.Map<MarkupResponse>(markup));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.Error(e, new
                {
                    e.Variable,
                    e.Value
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MarkupNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetPairId
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns list of settlement assets available for merchant according to general and personal asset settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchants/{merchantId}/settlementAssets")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolveSettlementAssets(string merchantId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IReadOnlyList<string> assets =
                    await _assetSettingsService.ResolveSettlementAsync(merchantId);

                return Ok(new AvailableAssetsResponseModel { Assets = assets });
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.Error(e, new
                {
                    e.Variable,
                    e.Value
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns list of payment assets available for merchant according to general and personal asset settings, settlement asset
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="settlementAssetId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentAssets")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolvePaymentAssets(string merchantId, [FromQuery] string settlementAssetId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            if (string.IsNullOrEmpty(settlementAssetId))
                return BadRequest(ErrorResponse.Create("Settlement asset id is invalid"));

            try
            {
                string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(settlementAssetId);

                Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

                if (asset == null)
                    return NotFound(ErrorResponse.Create("Couldn't find asset"));

                IReadOnlyList<string> assets =
                    await _assetSettingsService.ResolvePaymentAsync(merchantId, settlementAssetId);

                return Ok(new AvailableAssetsResponseModel { Assets = assets });
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.Error(e, new
                {
                    e.Variable,
                    e.Value
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, new { e.Asset });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
