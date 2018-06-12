using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.AssetRates;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/assetRates")]
    public class AssetRatesController : Controller
    {
        private readonly IAssetRatesService _assetRatesService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetRatesController(
            [NotNull] IAssetRatesService assetRatesService,
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver,
            [NotNull] ILog log)
        {
            _assetRatesService = assetRatesService ?? throw new ArgumentNullException(nameof(assetRatesService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
            _log = log.CreateComponentScope(nameof(AssetRatesController)) ??
                   throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Adds new current asset pair rate
        /// </summary>
        /// <param name="request">New asset pair rate request details</param>
        /// <response code="200">Asset pair rate details</response>
        /// <response code="404">Base or quoting asset not found</response>
        [HttpPost]
        [SwaggerOperation("AddAsseetRate")]
        [ProducesResponseType(typeof(AssetRateResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> Add([FromBody] AddAssetRateModel request)
        {
            try
            {
                string lykkeBaseAssetId = await _lykkeAssetsResolver.GetLykkeId(request.BaseAssetId);

                if (await _assetsLocalCache.GetAssetByIdAsync(lykkeBaseAssetId) == null)
                    return NotFound(ErrorResponse.Create("Base asset not found"));

                string lykkeQuotingAssetId = await _lykkeAssetsResolver.GetLykkeId(request.QuotingAssetId);

                if (await _assetsLocalCache.GetAssetByIdAsync(lykkeQuotingAssetId) == null)
                    return NotFound(ErrorResponse.Create("Quoting asset not found"));

                IAssetPairRate newRate =
                    await _assetRatesService.AddAsync(Mapper.Map<AddAssetPairRateCommand>(request));

                return Ok(Mapper.Map<AssetRateResponse>(newRate));
            }
            catch (AssetUnknownException e)
            {
                _log.WriteError(nameof(Add), new {e.Asset}, e);

                return NotFound(ErrorResponse.Create($"Asset not found [{e.Asset}]"));
            }
        }

        /// <summary>
        /// Returns current rate for asset pair given
        /// </summary>
        /// <param name="baseAssetId">Base asset id</param>
        /// <param name="quotingAssetId">Quoting asset id</param>
        /// <response code="200">Asset pair rate details</response>
        /// <response code="404">Base asset, quoting asset or rate not found</response>
        [HttpGet]
        [Route("{baseAssetId}/{quotingAssetId}")]
        [SwaggerOperation("GetCurrentAssetPairRate")]
        [ProducesResponseType(typeof(AssetRateResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCurrent(string baseAssetId, string quotingAssetId)
        {
            try
            {
                string lykkeBaseAssetId = await _lykkeAssetsResolver.GetLykkeId(Uri.UnescapeDataString(baseAssetId));

                if (await _assetsLocalCache.GetAssetByIdAsync(lykkeBaseAssetId) == null)
                    return NotFound(ErrorResponse.Create("Base asset not found"));

                string lykkeQuotingAssetId =
                    await _lykkeAssetsResolver.GetLykkeId(Uri.UnescapeDataString(quotingAssetId));

                if (await _assetsLocalCache.GetAssetByIdAsync(lykkeQuotingAssetId) == null)
                    return NotFound(ErrorResponse.Create("Quoting asset not found"));

                IAssetPairRate rate = await _assetRatesService.GetCurrentRate(baseAssetId, quotingAssetId);

                if (rate == null)
                    return NotFound(ErrorResponse.Create("Rate not found"));

                return Ok(Mapper.Map<AssetRateResponse>(rate));
            }
            catch (AssetUnknownException e)
            {
                _log.WriteError(nameof(GetCurrent), new {e.Asset}, e);

                return NotFound(ErrorResponse.Create($"Asset not found [{e.Asset}]"));
            }
        }
    }
}
