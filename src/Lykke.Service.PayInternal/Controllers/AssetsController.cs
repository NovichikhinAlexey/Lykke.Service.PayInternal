using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core;
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
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IMerchantService _merchantService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetsController(
            IAssetSettingsService assetSettingsService,
            IAssetsLocalCache assetsLocalCache,
            IMerchantService merchantService,
            ILog log,
            ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _assetSettingsService = assetSettingsService ??
                                    throw new ArgumentNullException(nameof(assetSettingsService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        /// <summary>
        /// Returns general asset settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("settings/general")]
        [SwaggerOperation(nameof(GetAssetGeneralSettings))]
        [ProducesResponseType(typeof(IEnumerable<AssetGeneralSettingsResponseModel>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssetGeneralSettings()
        {
            try
            {
                IReadOnlyList<IAssetGeneralSettings> assets = await _assetSettingsService.GetGeneralAsync();

                return Ok(Mapper.Map<IReadOnlyList<AssetGeneralSettingsResponseModel>>(assets));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAssetGeneralSettings), ex);

                throw;
            }
        }

        /// <summary>
        /// Updates general asset settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("settings/general")]
        [SwaggerOperation(nameof(SetAssetGeneralSettings))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetAssetGeneralSettings([FromBody] UpdateAssetGeneralSettingsRequest request)
        {
            try
            {
                string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(request.AssetDisplayId);

                Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

                if (asset == null)
                    return NotFound(ErrorResponse.Create($"Asset {request.AssetDisplayId} not found"));

                await _assetSettingsService.SetGeneralAsync(Mapper.Map<AssetGeneralSettings>(request));

                return NoContent();
            }
            catch (AssetUnknownException assetEx)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAssetGeneralSettings),
                    new {assetEx.Asset}.ToJson(), assetEx);

                return NotFound(ErrorResponse.Create($"Asset {assetEx.Asset} can't be resolved"));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAssetGeneralSettings), ex);

                throw;
            }
        }

        /// <summary>
        /// Returns personal asset availability settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("settings/merchant")]
        [SwaggerOperation("GetAssetsPersonalSettings")]
        [ProducesResponseType(typeof(AssetMerchantSettingsResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAssetMerchantSettings([FromQuery] string merchantId)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Couldn't find merchant"));

            try
            {
                IAssetMerchantSettings personal = await _assetSettingsService.GetByMerchantAsync(merchantId);

                return Ok(Mapper.Map<AssetMerchantSettingsResponse>(personal));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAssetMerchantSettings), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates personal asset availability settings
        /// </summary>
        /// <param name="settingsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("settings/merchant")]
        [SwaggerOperation("SetAssetsPersonalSettings")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetAssetMerchantSettings(
            [FromBody] UpdateAssetMerchantSettingsRequest settingsRequest)
        {
            IMerchant merchant = await _merchantService.GetAsync(settingsRequest.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Couldn't find merchant"));

            try
            {
                await _assetSettingsService.SetByMerchantAsync(settingsRequest.MerchantId,
                    settingsRequest.PaymentAssets,
                    settingsRequest.SettlementAssets);

                return NoContent();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAssetMerchantSettings), ex);
                throw;
            }
        }
    }
}
