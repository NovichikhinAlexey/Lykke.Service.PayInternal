using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Assets;
using Lykke.Service.PayInternal.Models.Markups;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class MerchantsController : Controller
    {
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IMerchantService _merchantService;
        private readonly IMarkupService _markupService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public MerchantsController(
            [NotNull] IMerchantService merchantService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] ILogFactory logFactory,
            [NotNull] IMarkupService markupService,
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _assetSettingsService =
                assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _log = logFactory.CreateLog(this);
            _markupService = markupService ?? throw new ArgumentNullException(nameof(markupService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        /// <summary>
        /// Returns all merchants.
        /// </summary>
        /// <returns>The collection of merchants.</returns>
        /// <response code="200">The collection of merchants.</response>
        [HttpGet]
        [Route("merchants")]
        [SwaggerOperation("MerchantsGetAll")]
        [ProducesResponseType(typeof(List<MerchantModel>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync()
        {
            IReadOnlyList<IMerchant> merchants = await _merchantService.GetAsync();

            var model = Mapper.Map<List<MerchantModel>>(merchants);

            return Ok(model);
        }

        /// <summary>
        /// Returns merchant.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The merchant.</returns>
        /// <response code="200">The merchant.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpGet]
        [Route("merchants/{merchantId}")]
        [SwaggerOperation("MerchantsGetById")]
        [ProducesResponseType(typeof(MerchantModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(string merchantId)
        {
            try
            {
                IMerchant merchant = await _merchantService.GetAsync(Uri.UnescapeDataString(merchantId));

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

                return Ok(Mapper.Map<MerchantModel>(merchant));
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
        /// Creates merchant.
        /// </summary>
        /// <returns>The created merchant.</returns>
        /// <param name="request">The merchant create request.</param>
        /// <response code="200">The created merchant.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants")]
        [SwaggerOperation("MerchantsCreate")]
        [ProducesResponseType(typeof(MerchantModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreateAsync([FromBody] CreateMerchantRequest request)
        {
            try
            {
                var merchant = Mapper.Map<Merchant>(request);

                IMerchant createdMerchant = await _merchantService.CreateAsync(merchant);

                return Ok(Mapper.Map<MerchantModel>(createdMerchant));
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
            catch (Exception exception) when (exception is DuplicateMerchantNameException ||
                                              exception is DuplicateMerchantApiKeyException)
            {
                _log.Warning(exception.Message, context: request);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }
        }

        /// <summary>
        /// Updates a merchant.
        /// </summary>
        /// <param name="request">The merchant update request.</param>
        /// <response code="204">The merchant successfully updated.</response>
        /// <response code="400">Invalid model.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpPatch]
        [Route("merchants")] // TODO: merchants/{merchantId} when Refit can use path parameter and body togather
        [SwaggerOperation("MerchantsUpdate")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateMerchantRequest request)
        {
            try
            {
                var merchant = Mapper.Map<Merchant>(request);

                await _merchantService.UpdateAsync(merchant);
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
            catch (MerchantNotFoundException exception)
            {
                _log.Warning(exception.Message, context: request);

                return NotFound(ErrorResponse.Create(exception.Message));
            }

            return NoContent();
        }

        /// <summary>
        /// Sets merchant public key.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="file">The public key file.</param>
        /// <response code="204">The public key successfully updated.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/publickey")]
        [SwaggerOperation("MerchantsSetPublicKey")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetPublicKeyAsync(string merchantId, IFormFile file)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            if (file == null || file.Length == 0)
                return BadRequest(ErrorResponse.Create("Empty file"));

            try
            {
                var fileContent = await file.OpenReadStream().ToBytesAsync();
                string publicKey = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

                await _merchantService.SetPublicKeyAsync(merchantId, publicKey);

                return NoContent();
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
            catch (MerchantNotFoundException e)
            {
                _log.Warning(e.Message, context: new {MerchantId = merchantId});

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Deletes a merchant.
        /// </summary>
        /// <param name="merchantId">The merchan id.</param>
        /// <response code="204">Merchant successfully deleted.</response>
        [HttpDelete]
        [Route("merchants/{merchantId}")]
        [SwaggerOperation("MerchantsDelete")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(string merchantId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

                await _merchantService.DeleteAsync(merchantId);

                return NoContent();
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
        /// Returns list of assets available for merchant according to availability type and general asset settings
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="type">Availability type</param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchants/{merchantId}/assets")]
        [SwaggerOperation("ResolveAssetsByMerchant")]
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [Obsolete("Use ResolveSettlementAssetsAsync and ResolvePaymentAssetsAsync instead")]
        public async Task<IActionResult> ResolveAssetsByMerchant(string merchantId,
            [FromQuery] AssetAvailabilityType type)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

                IReadOnlyList<string> resolvedAssets =
                    await _assetSettingsService.ResolveAsync(merchantId, type);

                return Ok(new AvailableAssetsResponseModel {Assets = resolvedAssets});
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
        /// Returns markup values for merchant and asset pair according to merchant's and default's settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("merchants/{merchantId}/markups/{assetPairId}")]
        [SwaggerOperation("ResolveMarkupByMerchant")]
        [ProducesResponseType(typeof(MarkupResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolveMarkupByMerchant(string merchantId, string assetPairId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);
            assetPairId = Uri.UnescapeDataString(assetPairId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

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
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolveSettlementAssets(string merchantId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

                IReadOnlyList<string> assets =
                    await _assetSettingsService.ResolveSettlementAsync(merchantId);

                return Ok(new AvailableAssetsResponseModel {Assets = assets});
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
        [ProducesResponseType(typeof(AvailableAssetsResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResolvePaymentAssets(string merchantId, [FromQuery] string settlementAssetId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            if (string.IsNullOrEmpty(settlementAssetId))
                return BadRequest(ErrorResponse.Create("Settlement asset id is invalid"));

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Couldn't find merchant"));

                string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(settlementAssetId);

                Asset asset = await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId);

                if (asset == null)
                    return NotFound(ErrorResponse.Create("Couldn't find asset"));

                IReadOnlyList<string> assets =
                    await _assetSettingsService.ResolvePaymentAsync(merchantId, settlementAssetId);

                return Ok(new AvailableAssetsResponseModel {Assets = assets});
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
                _log.Error(e, new {e.Asset});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
