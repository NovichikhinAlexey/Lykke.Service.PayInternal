using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Assets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/assets")]
    public class AssetsController : Controller
    {
        private readonly IAssetsAvailabilityService _assetsAvailabilityService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IMerchantService _merchantService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetsController(
            IAssetsAvailabilityService assetsAvailabilityService,
            IAssetsLocalCache assetsLocalCache,
            IMerchantService merchantService,
            ILog log, 
            ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _assetsAvailabilityService = assetsAvailabilityService ??
                                         throw new ArgumentNullException(nameof(assetsAvailabilityService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        /// <summary>
        /// Returns general asset availability settings by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("settings/general")]
        [SwaggerOperation("GetAssetsSettings")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetGeneralAssetsSettings([FromQuery] AssetAvailabilityType type)
        {
            try
            {
                IReadOnlyList<IAssetAvailability> assets = await _assetsAvailabilityService.GetGeneralByTypeAsync(type);

                return Ok(new AvailableAssetsResponseModel {Assets = assets.Select(x => x.AssetId).ToList()});
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetGeneralAssetsSettings), ex);

                throw;
            }
        }

        /// <summary>
        /// Updates general asset availability settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("settings/general")]
        [SwaggerOperation("SetAssetsSettings")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetGeneralAssetsSettings([FromBody] UpdateAssetAvailabilityRequest request)
        {
            try
            {
                string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(request.AssetId);

                Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

                if (asset == null)
                    return NotFound(ErrorResponse.Create($"Asset {request.AssetId} not found"));

                await _assetsAvailabilityService.SetGeneralAsync(request.AssetId, request.AvailabilityType,
                    request.Value);

                return NoContent();
            }
            catch (AssetUnknownException assetEx)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetGeneralAssetsSettings),
                    new {assetEx.Asset}.ToJson(), assetEx);

                return NotFound(ErrorResponse.Create($"Asset {assetEx.Asset} can't be resolved"));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetGeneralAssetsSettings), ex);

                throw;
            }
        }

        /// <summary>
        /// Returns personal asset availability settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("settings/personal")]
        [SwaggerOperation("GetAssetsPersonalSettings")]
        [ProducesResponseType(typeof(AssetAvailabilityByMerchantResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAssetsPersonalSettings([FromQuery] string merchantId)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Couldn't find merchant"));

            try
            {
                IAssetAvailabilityByMerchant personal = await _assetsAvailabilityService.GetPersonalAsync(merchantId);

                return Ok(Mapper.Map<AssetAvailabilityByMerchantResponse>(personal));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAssetsPersonalSettings), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates personal asset availability settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("settings/personal")]
        [SwaggerOperation("SetAssetsPersonalSettings")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetAssetsPersonalSettings([FromBody] UpdateAssetAvailabilityByMerchantRequest request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Couldn't find merchant"));

            try
            {
                await _assetsAvailabilityService.SetPersonalAsync(request.MerchantId, request.PaymentAssets,
                    request.SettlementAssets);

                return NoContent();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAssetsPersonalSettings), ex);
                throw;
            }
        }
    }
}
