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
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Assets;
using LykkePay.Common.Validation;
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
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetsController(
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] ILogFactory logFactory,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _log = logFactory.CreateLog(this);
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
            IReadOnlyList<IAssetGeneralSettings> assets = await _assetSettingsService.GetGeneralAsync();

            return Ok(Mapper.Map<IReadOnlyList<AssetGeneralSettingsResponseModel>>(assets));
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
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Asset not found"));
            }
            catch (AssetUnknownException e)
            {
                _log.ErrorWithDetails(e, new {e.Asset});

                return NotFound(ErrorResponse.Create($"Asset {e.Asset} can't be resolved"));
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
            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant id is invalid"));

            try
            {
                IAssetMerchantSettings personal = await _assetSettingsService.GetByMerchantAsync(merchantId);

                return Ok(Mapper.Map<AssetMerchantSettingsResponse>(personal));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Merchant not found"));
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
            try
            {
                await _assetSettingsService.SetByMerchantAsync(settingsRequest.MerchantId,
                    settingsRequest.PaymentAssets,
                    settingsRequest.SettlementAssets);

                return NoContent();
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Merchant not found"));
            }
        }
    }
}
