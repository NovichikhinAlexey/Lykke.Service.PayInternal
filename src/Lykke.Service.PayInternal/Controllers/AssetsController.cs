using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class AssetsController : Controller
    {
        private readonly IAssetsAvailabilityService _assetsAvailabilityService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILog _log;

        public AssetsController(IAssetsAvailabilityService assetsAvailabilityService, IAssetsLocalCache assetsLocalCache,  ILog log)
        {
            _assetsAvailabilityService = assetsAvailabilityService ??
                                         throw new ArgumentNullException(nameof(assetsAvailabilityService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns list of assets which are allowed to be used for payment or settlement
        /// </summary>
        /// <param name="availabilityType">The availability type. Possible values: Payment, Settlement</param>
        /// <returns></returns>
        [HttpGet]
        [Route("available")]
        [SwaggerOperation("GetAvailable")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailable([FromQuery] AssetAvailabilityType availabilityType)
        {
            try
            {
                IReadOnlyList<IAssetAvailability> result = await _assetsAvailabilityService.GetAvailableAsync(availabilityType);
                
                var assets = new List<string>();

                foreach (IAssetAvailability assetAvailability in result)
                {
                    Asset asset = await _assetsLocalCache.GetAssetByIdAsync(assetAvailability.AssetId);

                    assets.Add(asset.Id);
                }

                var response = new AvailableAssetsResponseModel { Assets = assets };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAvailable), ex);
                throw;
            }
        }
        /// <summary>
        /// Returns list of assets which are allowed to be used for payment or settlement
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("availablebymerchant")]
        [SwaggerOperation("GetAvailableByMerchant")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailableByMerchant([FromQuery] string merchantId, AssetAvailabilityType assetAvailabilityType)
        {
            try
            {
                IReadOnlyList<IAssetAvailability> result = new List<IAssetAvailability>();
                IReadOnlyList<IAssetAvailability> resultMerchant = await _assetsAvailabilityService.GetAvailableAsync(merchantId, assetAvailabilityType);
                IReadOnlyList<IAssetAvailability> resultGlobal = await _assetsAvailabilityService.GetAvailableAsync(assetAvailabilityType);
                var assetsFromSettings = await _assetsAvailabilityService.GetAssetsAvailabilityFromSettings(assetAvailabilityType);
                if (resultMerchant == null)
                {
                    if (assetsFromSettings.Count == 0)
                        result = resultGlobal;
                    else
                        result = assetsFromSettings;
                }
                else
                    result = resultMerchant;
                var assets = new List<string>();

                foreach (IAssetAvailability assetAvailability in result)
                {
                    Asset asset = await _assetsLocalCache.GetAssetByIdAsync(assetAvailability.AssetId);

                    assets.Add(asset.Id);
                }

                var response = new AvailableAssetsResponseModel { Assets = assets };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAvailable), ex);
                throw;
            }
        }
        /// <summary>
        /// Return personal settings available assets by merchant
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("availablepersonal")]
        [SwaggerOperation("GetAvailablePersonal")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailableByMerchant([FromQuery] string merchantId)
        {
            try
            {
                var response = await _assetsAvailabilityService.GetAvailablePersonalAsync(merchantId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAvailable), ex);
                throw;
            }
        }
        /// <summary>
        /// Updates asset availability with provided availability type and value
        /// </summary>
        /// <param name="request">Contains availability type which has the following possible values: Payment, Settlement</param>
        /// <returns></returns>
        [HttpPost]
        [Route("available")]
        [SwaggerOperation("SetAvailability")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetAvailability([FromBody] UpdateAssetAvailabilityRequest request)
        {
            Asset asset = await _assetsLocalCache.GetAssetByIdAsync(request.AssetId);

            if (asset == null)
                return NotFound(ErrorResponse.Create($"Asset {request.AssetId} not found"));

            try
            {
                await _assetsAvailabilityService.UpdateAsync(request.AssetId, request.AvailabilityType, request.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAvailability), ex);
                throw;
            }
        }
        /// <summary>
        /// Updates asset availability assets by merchant
        /// </summary>
        /// <param name="request">Contains availability type which has the following possible values: Payment, Settlement</param>
        /// <returns></returns>
        [HttpPost]
        [Route("availablebymerchant")]
        [SwaggerOperation("SetAvailabilityByMerchant")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetAvailabilityByMerchant([FromBody] UpdateAssetAvailabilityByMerchantRequest request)
        {
            try
            {
                await _assetsAvailabilityService.UpdateMerchantAssetsAsync(request.PaymentAssets, request.SettlementAssets, request.MerchantId);

                return NoContent();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(SetAvailability), ex);
                throw;
            }
        }
    }
}
