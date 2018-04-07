using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
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
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    //todo: rethink controller implementation
    //too much contracts having now
    //Probably, in most cases we should use PaymentRequestDetailsModel as response 
    public class PaymentRequestsController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IOrderService _orderService;
        private readonly ITransactionsService _transactionsService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IRefundService _refundService;
        private readonly ILog _log;

        public PaymentRequestsController(
            IPaymentRequestService paymentRequestService,
            IOrderService orderService,
            ITransactionsService transactionsService,
            IAssetsLocalCache assetsLocalCache,
            IRefundService refundService,
            ILog log)
        {
            _paymentRequestService = paymentRequestService;
            _orderService = orderService;
            _transactionsService = transactionsService;
            _assetsLocalCache = assetsLocalCache;
            _refundService = refundService;
            _log = log;
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

                IOrder order = await _orderService.GetAsync(paymentRequestId, paymentRequest.OrderId);

                IReadOnlyList<IPaymentRequestTransaction> paymentTransactions =
                    (await _transactionsService.GetAsync(paymentRequest.WalletAddress)).Where(x => x.IsPayment())
                    .ToList();

                PaymentRequestRefund refund = await _paymentRequestService.GetRefundInfoAsync(paymentRequestId);

                var model = Mapper.Map<PaymentRequestDetailsModel>(paymentRequest);
                model.Order = Mapper.Map<PaymentRequestOrderModel>(order);
                model.Transactions = Mapper.Map<List<PaymentRequestTransactionModel>>(paymentTransactions);
                model.Refund = Mapper.Map<PaymentRequestRefundModel>(refund);

                return Ok(model);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(GetDetailsAsync),
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
        public async Task<IActionResult> CreateAsync([FromBody] CreatePaymentRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (await _assetsLocalCache.GetAssetByIdAsync(model.SettlementAssetId) == null)
                return BadRequest(ErrorResponse.Create("Settlement asset doesn't exist"));

            if (await _assetsLocalCache.GetAssetByIdAsync(model.PaymentAssetId) == null)
                return BadRequest(ErrorResponse.Create("Payment asset doesn't exist"));

            if (await _assetsLocalCache.GetAssetPairAsync(model.PaymentAssetId, model.SettlementAssetId) == null)
                return BadRequest(ErrorResponse.Create("Asset pair doesn't exist"));

            try
            {
                var paymentRequest = Mapper.Map<PaymentRequest>(model);

                IPaymentRequest createdPaymentRequest = await _paymentRequestService.CreateAsync(paymentRequest);

                return Ok(Mapper.Map<PaymentRequestModel>(createdPaymentRequest));
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(CreateAsync), model.ToJson(),
                    exception);
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
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(RefundAsync), request.ToJson(), ex);

                if (ex is RefundValidationException validationEx)
                {
                    return BadRequest(new RefundErrorModel {Code = validationEx.ErrorType});
                }
                
                return BadRequest(new RefundErrorModel {Code = RefundErrorType.Unknown});
            }
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
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(CancelAsync), new
                {
                    merchantId,
                    paymentRequestId
                }.ToJson(), ex);

                if (ex is PaymentRequestNotFoundException notFoundEx)
                    return NotFound(ErrorResponse.Create(notFoundEx.Message));

                if (ex is NotAllowedStatusException notAllowedEx)
                    return BadRequest(ErrorResponse.Create(notAllowedEx.Message));

                throw;
            }
        }
    }
}
