using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.MerchantWallets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/merchantWallets")]
    public class MerchantWalletsController : Controller
    {
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly IMerchantService _merchantService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public MerchantWalletsController(
            [NotNull] IMerchantWalletService merchantWalletService,
            [NotNull] IMerchantService merchantService,
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver,
            [NotNull] ILog log)
        {
            _merchantWalletService =
                merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
            _log = log.CreateComponentScope(nameof(MerchantWalletsController)) ??
                   throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates new merchant wallet
        /// </summary>
        /// <param name="request">Merchant wallet creation details</param>
        /// <response code="200">Merchant wallet details</response>
        /// <response code="400">Merchant not found or network given is not supported</response>
        /// <response code="404">Asset or Blockchain client implementation not found</response>
        /// <response code="502">Blockchain API error</response>
        [HttpPost]
        [SwaggerOperation("CreateMerchantWallet")]
        [ProducesResponseType(typeof(MerchantWalletResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.BadGateway)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Create([FromBody] CreateMerchantWalletModel request)
        {
            if (!request.Network.HasValue || request.Network == BlockchainType.None)
                return BadRequest(ErrorResponse.Create("Invalid network value, possible values are: Bitcoin, Ethereum, EthereumIata"));

            try
            {
                IMerchantWallet wallet =
                    await _merchantWalletService.CreateAsync(Mapper.Map<CreateMerchantWalletCommand>(request));

                return Ok(Mapper.Map<MerchantWalletResponse>(wallet));
            }
            catch (InvalidOperationException e)
            {
                _log.WriteError(nameof(Create), request, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletAddressAllocationException e)
            {
                _log.WriteError(nameof(Create), new {e.Blockchain}, e);

                return StatusCode((int) HttpStatusCode.BadGateway);
            }
            catch (UnrecognizedApiResponse e)
            {
                _log.WriteError(nameof(Create), new {e.ResponseType}, e);

                return StatusCode((int) HttpStatusCode.BadGateway);
            }
        }

        /// <summary>
        /// Deletes merchant wallet
        /// </summary>
        /// <param name="merchantWalletId">Merchant wallet id</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="404">Merchant wallet not found</response>
        [HttpDelete]
        [Route("{merchantWalletId}")]
        [SwaggerOperation("DeleteMerchantWallet")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(string merchantWalletId)
        {
            try
            {
                await _merchantWalletService.DeleteAsync(Uri.UnescapeDataString(merchantWalletId));

                return NoContent();
            }
            catch (MerchantWalletIdNotFoundException e)
            {
                _log.WriteError(nameof(Delete), new {e.MerchantWalletId}, e);

                return NotFound(ErrorResponse.Create("Merchant wallet not found"));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.WriteError(nameof(Delete), new
                {
                    e.Variable,
                    e.Value
                }, e);

                return NotFound(ErrorResponse.Create("Merchant wallet not found"));
            }
        }

        /// <summary>
        /// Updates default assets for merchant wallet
        /// </summary>
        /// <param name="request">Merchant wallet default assets update details</param>
        /// <returns></returns>
        /// <response code="204">Successfully updated</response>
        /// <response code="404">Asset or Merchant wallet not found</response>
        [HttpPost]
        [Route("defaultAssets")]
        [SwaggerOperation("SetMerchantWalletDefaultAssets")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> SetDefaultAssets([FromBody] UpdateMerchantWalletDefaultAssetsModel request)
        {
            if (!request.Network.HasValue || request.Network == BlockchainType.None)
                return BadRequest(ErrorResponse.Create("Invalid network value, possible values are: Bitcoin, Ethereum, EthereumIata"));

            var assetsToValidate = new List<string>();

            if (request.IncomingPaymentDefaults != null)
                assetsToValidate.AddRange(request.IncomingPaymentDefaults);

            if (request.OutgoingPaymentDefaults != null)
                assetsToValidate.AddRange(request.OutgoingPaymentDefaults);

            try
            {
                foreach (string assetToValidateId in assetsToValidate)
                {
                    string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(assetToValidateId);

                    if (await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId) == null)
                        throw new AssetUnknownException(lykkeAssetId);
                }

                await _merchantWalletService.SetDefaultAssetsAsync(
                    request.MerchantId,
                    request.Network.Value,
                    request.WalletAddress,
                    request.IncomingPaymentDefaults,
                    request.OutgoingPaymentDefaults);

                return NoContent();
            }
            catch (AssetUnknownException e)
            {
                _log.WriteError(nameof(SetDefaultAssets), new {e.Asset}, e);

                return NotFound(ErrorResponse.Create($"Asset not found [{e.Asset}]"));
            }
            catch (MerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(SetDefaultAssets), new
                {
                    e.MerchantId,
                    e.Network,
                    e.WalletAddress
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns list of merchant wallets
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        /// <response code="200">List of merchant wallets</response>
        /// <response code="404">Merchant not found</response>
        [HttpGet]
        [Route("{merchantId}")]
        [SwaggerOperation("GetMerchantWallets")]
        [ProducesResponseType(typeof(IEnumerable<MerchantWalletResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByMerchant(string merchantId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Merchant not found"));

                IReadOnlyList<IMerchantWallet> wallets = await _merchantWalletService.GetByMerchantAsync(merchantId);

                return Ok(Mapper.Map<IEnumerable<MerchantWalletResponse>>(wallets));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.WriteError(nameof(GetByMerchant), new
                {
                    e.Variable,
                    e.Value
                }, e);

                return NotFound(ErrorResponse.Create("Merchant not found"));
            }
        }

        /// <summary>
        /// Returns default merchant wallet for given asset and payment direction
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        /// <param name="assetId">The asset id</param>
        /// <param name="paymentDirection">The payment direction</param>
        /// <response code="200">Merchant wallet details</response>
        /// <response code="400">The blockchain network is not defined for the asset</response>
        /// <response code="404">Merchant, Asset or default wallet not found</response>
        [HttpGet]
        [Route("{merchantId}/default")]
        [SwaggerOperation("GetDefaultMerchantWallet")]
        [ProducesResponseType(typeof(MerchantWalletResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDefault(string merchantId, [FromQuery] string assetId,
            [FromQuery] PaymentDirection? paymentDirection)
        {
            if (string.IsNullOrEmpty(assetId))
                return BadRequest(ErrorResponse.Create("AssetId should not be empty"));

            if (paymentDirection == null)
                return BadRequest(ErrorResponse.Create("PaymentDirection should not be empty"));

            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Merchant not found"));

                string lykkeAssetId = await _lykkeAssetsResolver.GetLykkeId(assetId);

                if (await _assetsLocalCache.GetAssetByIdAsync(lykkeAssetId) == null)
                    return NotFound(ErrorResponse.Create("Asset not found"));

                IMerchantWallet wallet =
                    await _merchantWalletService.GetDefaultAsync(merchantId, assetId, paymentDirection.Value);

                return Ok(Mapper.Map<MerchantWalletResponse>(wallet));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.WriteError(nameof(GetDefault), new
                {
                    e.Variable,
                    e.Value
                }, e);

                return NotFound(ErrorResponse.Create("Merchant not found"));
            }
            catch (AssetUnknownException e)
            {
                _log.WriteError(nameof(GetDefault), new { e.Asset }, e);

                return NotFound(ErrorResponse.Create($"Asset not found [{e.Asset}]"));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.WriteError(nameof(GetDefault), new {e.AssetId}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.WriteError(nameof(GetDefault), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(GetDefault), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns balances for all merchant's wallets
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <response code="200">List of balances</response>
        /// <response code="404">Merchant not found</response>
        /// <response code="501">Blockchain client implementation not found</response>
        [HttpGet]
        [Route("balances/{merchantId}")]
        [SwaggerOperation("GetMerchantWalletBalances")]
        [ProducesResponseType(typeof(IEnumerable<MerchantWalletBalanceResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotImplemented)]
        public async Task<IActionResult> GetBalances(string merchantId)
        {
            merchantId = Uri.UnescapeDataString(merchantId);

            try
            {
                IMerchant merchant = await _merchantService.GetAsync(merchantId);

                if (merchant == null)
                    return NotFound(ErrorResponse.Create("Merchant not found"));

                IReadOnlyList<MerchantWalletBalanceLine> balances =
                    await _merchantWalletService.GetBalancesAsync(merchantId);

                return Ok(Mapper.Map<IEnumerable<MerchantWalletBalanceResponse>>(balances));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.WriteError(nameof(GetBalances), new
                {
                    e.Variable,
                    e.Value
                }, e);

                return NotFound(ErrorResponse.Create("Merchant not found"));
            }
            catch (InvalidOperationException e)
            {
                _log.WriteError(nameof(GetBalances), new {merchantId}, e);

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
        }
    }
}
