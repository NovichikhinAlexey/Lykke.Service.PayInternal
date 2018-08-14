using AutoMapper;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
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
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;
using Lykke.Service.PayInternal.Core;

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
            [NotNull] ILogFactory logFactory,
            [NotNull] IPaymentRequestDetailsBuilder paymentRequestDetailsBuilder,
            [NotNull] IMerchantService merchantService,
			[NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _paymentRequestDetailsBuilder = paymentRequestDetailsBuilder ?? throw new ArgumentNullException(nameof(paymentRequestDetailsBuilder));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _log = logFactory.CreateLog(this);
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
        [Route("merchants/paymentrequests")]
        [SwaggerOperation("PaymentRequestsCreate")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentRequestErrorModel), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePaymentRequestModel model)
        {
            try
            {
                IReadOnlyList<string> settlementAssets =
                    await _assetSettingsService.ResolveSettlementAsync(model.MerchantId);

                if (!settlementAssets.Contains(model.SettlementAssetId))
                    return BadRequest(new PaymentRequestErrorModel
                        {Code = CreatePaymentRequestErrorType.SettlementAssetNotAvailable});

                IReadOnlyList<string> paymentAssets =
                    await _assetSettingsService.ResolvePaymentAsync(model.MerchantId, model.SettlementAssetId);

                if (!paymentAssets.Contains(model.PaymentAssetId))
                    return BadRequest(new PaymentRequestErrorModel
                        {Code = CreatePaymentRequestErrorType.PaymentAssetNotAvailable});

                var paymentRequest = Mapper.Map<PaymentRequest>(model);

                IPaymentRequest createdPaymentRequest = await _paymentRequestService.CreateAsync(paymentRequest);

                return Ok(Mapper.Map<PaymentRequestModel>(createdPaymentRequest));
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, e.Message, e.Asset);

                return BadRequest(new PaymentRequestErrorModel
                    {Code = CreatePaymentRequestErrorType.RequestValidationCommonError});
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, e.Message, e.AssetId);

                return BadRequest(new PaymentRequestErrorModel
                    {Code = CreatePaymentRequestErrorType.RequestValidationCommonError});
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
            catch (RefundOperationFailedException e)
            {
                _log.Error(e, new {errors = e.TransferErrors});
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, new {e.Asset});
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, new {e.AssetId});
            }
            catch (Exception e)
            {
                _log.Error(e, request);

                if (e is RefundValidationException validationEx)
                {
                    _log.Error(e, new {validationEx.ErrorType});

                    return BadRequest(new RefundErrorModel {Code = validationEx.ErrorType});
                }
            }

            return BadRequest(new RefundErrorModel {Code = RefundErrorType.Unknown});
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
            catch (Exception e)
            {
                _log.Error(e, new
                {
                    merchantId,
                    paymentRequestId
                });

                if (e is PaymentRequestNotFoundException notFoundEx)
                {
                    _log.Error(notFoundEx, new
                    {
                        notFoundEx.WalletAddress,
                        notFoundEx.MerchantId,
                        notFoundEx.PaymentRequestId
                    });

                    return NotFound(ErrorResponse.Create(notFoundEx.Message));
                }

                if (e is NotAllowedStatusException notAllowedEx)
                {
                    _log.Error(notAllowedEx,
                        new {status = notAllowedEx.Status.ToString()});

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
                _log.Error(e, new
                {
                    e.AssetId,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, new { e.AssetId });

                return StatusCode((int)HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.Error(e, new {e.WalletAddress});

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
                _log.Error(e, new
                {
                    e.AssetId,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, new {e.AssetId});

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.Error(e, new {e.WalletAddress});

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (PaymentOperationFailedException e)
            {
                _log.Error(e, new {errors = e.TransferErrors});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DistributedLockAcquireException e)
            {
                _log.Error(e, new {e.Key});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
