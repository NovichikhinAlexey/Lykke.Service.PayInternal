using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.PaymentRequests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    //todo: rethink controller implementation
    //too much contracts having now
    //Probably, in most cases we should use PaymentRequestDetailsModel as response 
    public class PaymentRequestsController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IRefundService _refundService;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IPaymentRequestDetailsBuilder _paymentRequestDetailsBuilder;
        private readonly IMerchantService _merchantService;
        private readonly ILog _log;

        public PaymentRequestsController(
            [NotNull] IPaymentRequestService paymentRequestService,
            [NotNull] IRefundService refundService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] ILog log,
            [NotNull] IPaymentRequestDetailsBuilder paymentRequestDetailsBuilder,
            [NotNull] IMerchantService merchantService)
        {
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _paymentRequestDetailsBuilder = paymentRequestDetailsBuilder ?? throw new ArgumentNullException(nameof(paymentRequestDetailsBuilder));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _log = log.CreateComponentScope(nameof(PaymentRequestsController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Returns merchant payment requests.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of merchant payment requests.</returns>
        /// <response code="200">The collection of merchant payment requests.</response>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests")]
        [SwaggerOperation("PaymentRequestsGetAll")]
        [ProducesResponseType(typeof(List<PaymentRequestModel>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync(string merchantId)
        {
            IReadOnlyList<IPaymentRequest> paymentRequests = await _paymentRequestService.GetAsync(merchantId);

            var model = Mapper.Map<List<PaymentRequestModel>>(paymentRequests);

            return Ok(model);
        }

        /// <summary>
        /// Returns merchant payment request.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestsGetById")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(string merchantId, string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                return NotFound(ErrorResponse.Create("Couldn't find payment request"));

            var model = Mapper.Map<PaymentRequestModel>(paymentRequest);

            return Ok(model);
        }

        /// <summary>
        /// Returns merchant payment request details.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request details.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [Obsolete("Need to remove")]
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/details/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestDetailsGetById")]
        [ProducesResponseType(typeof(PaymentRequestDetailsModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDetailsAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

                if (paymentRequest == null)
                    return NotFound(ErrorResponse.Create("Could not find payment request"));

                PaymentRequestRefund refundInfo =
                    await _paymentRequestService.GetRefundInfoAsync(paymentRequest.WalletAddress);

                PaymentRequestDetailsModel model = await _paymentRequestDetailsBuilder.Build<
                    PaymentRequestDetailsModel, 
                    PaymentRequestOrderModel, 
                    PaymentRequestTransactionModel,
                    PaymentRequestRefundModel>(paymentRequest, refundInfo);

                return Ok(model);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(GetDetailsAsync),
                    new
                    {
                        MerchantId = merchantId,
                        PaymentRequestId = paymentRequestId
                    }.ToJson(), ex);

                throw;
            }
        }

        /// <summary>
        ///  Returns merchant payment request by wallet address
        /// </summary>
        /// <param name="walletAddress">Wallet address</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [HttpGet]
        [Route("paymentrequests/byAddress/{walletAddress}")]
        [SwaggerOperation("PaymentRequestGetByWalletAddress")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByAddressAsync(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.FindAsync(walletAddress);

            if (paymentRequest == null)
                return NotFound(ErrorResponse.Create("Couldn't find payment request by wallet address"));

            var model = Mapper.Map<PaymentRequestModel>(paymentRequest);

            return Ok(model);
        }

        /// <summary>
        /// Creates a payment request and wallet.
        /// </summary>
        /// <param name="model">The payment request creation information.</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/paymentrequests")] //TODO: merchants/{merchantId}/peymentrequests when Refit can use path parameter and body togather
        [SwaggerOperation("PaymentRequestsCreate")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePaymentRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                IReadOnlyList<string> settlementAssets =
                    await _assetSettingsService.ResolveSettlementAsync(model.MerchantId);

                if (!settlementAssets.Contains(model.SettlementAssetId))
                    return BadRequest(ErrorResponse.Create("Settlement asset is not available"));

                IReadOnlyList<string> paymentAssets =
                    await _assetSettingsService.ResolvePaymentAsync(model.MerchantId, model.SettlementAssetId);

                if (!paymentAssets.Contains(model.PaymentAssetId))
                    return BadRequest(ErrorResponse.Create("Payment asset is not available"));

                var paymentRequest = Mapper.Map<PaymentRequest>(model);

                IPaymentRequest createdPaymentRequest = await _paymentRequestService.CreateAsync(paymentRequest);

                return Ok(Mapper.Map<PaymentRequestModel>(createdPaymentRequest));
            }
            catch (AssetUnknownException assetEx)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(CreateAsync),
                    new {assetEx.Asset}.ToJson(), assetEx);

                return BadRequest(ErrorResponse.Create($"Asset {assetEx.Asset} can't be resolved"));
            }
            catch (AssetNetworkNotDefinedException networkEx)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(CreateAsync),
                    new {networkEx.AssetId}.ToJson(), networkEx);

                return BadRequest(ErrorResponse.Create(networkEx.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(CreateAsync), model.ToJson(), ex);

                throw;
            }
        }

        /// <summary>
        /// Starts refund process on payment request associated with source wallet address
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("merchants/paymentrequests/refunds")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(RefundResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(RefundErrorModel), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> RefundAsync([FromBody] RefundRequestModel request)
        {
            try
            {
                RefundResult refundResult = await _refundService.ExecuteAsync(request.MerchantId,
                    request.PaymentRequestId, request.DestinationAddress);

                return Ok(Mapper.Map<RefundResponseModel>(refundResult));
            }
            catch (RefundOperationFailedException refundFailedEx)
            {
                await _log.WriteErrorAsync(nameof(RefundAsync), new {errors = refundFailedEx.TransferErrors}.ToJson(),
                    refundFailedEx);
            }
            catch (AssetUnknownException assetEx)
            {
                await _log.WriteErrorAsync(nameof(RefundAsync), new {assetEx.Asset}.ToJson(), assetEx);
            }
            catch (AssetNetworkNotDefinedException networkEx)
            {
                await _log.WriteErrorAsync(nameof(RefundAsync), new {networkEx.AssetId}.ToJson(), networkEx);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(RefundAsync), request.ToJson(), ex);

                if (ex is RefundValidationException validationEx)
                {
                    await _log.WriteErrorAsync(nameof(RefundAsync),
                        new {errorType = validationEx.ErrorType.ToString()}.ToJson(), validationEx);

                    return BadRequest(new RefundErrorModel {Code = validationEx.ErrorType});
                }
            }

            return BadRequest(new RefundErrorModel { Code = RefundErrorType.Unknown });
        }

        /// <summary>
        /// Cancels the payment request
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        [SwaggerOperation("Cancel")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                await _paymentRequestService.CancelAsync(merchantId, paymentRequestId);

                return NoContent();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(CancelAsync), new
                {
                    merchantId,
                    paymentRequestId
                }.ToJson(), ex);

                if (ex is PaymentRequestNotFoundException notFoundEx)
                {
                    await _log.WriteErrorAsync(nameof(CancelAsync), new
                    {
                        notFoundEx.WalletAddress,
                        notFoundEx.MerchantId,
                        notFoundEx.PaymentRequestId
                    }.ToJson(), notFoundEx);

                    return NotFound(ErrorResponse.Create(notFoundEx.Message));
                }

                if (ex is NotAllowedStatusException notAllowedEx)
                {
                    await _log.WriteErrorAsync(nameof(CancelAsync),
                        new {status = notAllowedEx.Status.ToString()}.ToJson(), notAllowedEx);

                    return BadRequest(ErrorResponse.Create(notAllowedEx.Message));
                }

                throw;
            }
        }

        /// <summary>
        /// Validates payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <response code="204">Validated successfully</response>
        /// <response code="400">Insufficient funds</response>
        /// <response code="404">Payment request, merchant, payer merchant, default wallet or payment request wallet not found</response>
        /// <response code="501">Asset network support not implemented</response>
        [HttpPost]
        [Route("paymentrequests/prePayment")]
        [SwaggerOperation(nameof(PrePay))]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotImplemented)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> PrePay([FromBody] PrePaymentModel request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            IMerchant payer = await _merchantService.GetAsync(request.PayerMerchantId);

            if (payer == null)
                return NotFound(ErrorResponse.Create("Payer merchant not found"));

            try
            {
                await _paymentRequestService.PrePayAsync(Mapper.Map<PaymentCommand>(request));

                return NoContent();
            }
            catch (InsufficientFundsException e)
            {
                _log.WriteError(nameof(PrePay), new
                {
                    e.AssetId,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.WriteError(nameof(PrePay), new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.WriteError(nameof(PrePay), new { e.AssetId }, e);

                return StatusCode((int)HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.WriteError(nameof(PrePay), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(PrePay), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.WriteError(nameof(PrePay), new { e.WalletAddress }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Executes payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <response code="204">Payment executed successfully</response>
        /// <response code="400">Payment failed</response>
        /// <response code="404">Payment request, merchant, default wallet, payment request wallet not found or couldn't get payment request lock</response>
        /// <response code="501">Asset network support not implemented</response>
        [HttpPost]
        [Route("paymentrequests/payment")]
        [SwaggerOperation(nameof(Pay))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotImplemented)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Pay([FromBody] PaymentModel request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            IMerchant payer = await _merchantService.GetAsync(request.PayerMerchantId);

            if (payer == null)
                return NotFound(ErrorResponse.Create("Payer merchant not found"));

            try
            {
                await _paymentRequestService.PayAsync(Mapper.Map<PaymentCommand>(request));

                return NoContent();
            }
            catch (InsufficientFundsException e)
            {
                _log.WriteError(nameof(Pay), new
                {
                    e.AssetId,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.WriteError(nameof(Pay), new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.WriteError(nameof(Pay), new {e.AssetId}, e);

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.WriteError(nameof(Pay), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(Pay), new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                }, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.WriteError(nameof(Pay), new {e.WalletAddress}, e);

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (PaymentOperationFailedException e)
            {
                _log.WriteError(nameof(Pay), new {errors = e.TransferErrors}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DistributedLockAcquireException e)
            {
                _log.WriteError(nameof(Pay), new {e.Key}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
