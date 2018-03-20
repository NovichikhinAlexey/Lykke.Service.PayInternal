using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
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
        [ProducesResponseType(typeof(List<PaymentRequestModel>), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
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
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/details/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestDetailsGetById")]
        [ProducesResponseType(typeof(PaymentRequestDetailsModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDetailsAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

                if (paymentRequest == null)
                    return NotFound(ErrorResponse.Create("Could not find payment request by given merchant ID and payment request ID"));

                IOrder order = await _orderService.GetAsync(paymentRequestId, paymentRequest.OrderId);

                IReadOnlyList<IPaymentRequestTransaction> transactions =
                    (await _transactionsService.GetAsync(paymentRequest.WalletAddress)).ToList();

                var model = Mapper.Map<PaymentRequestDetailsModel>(paymentRequest);
                model.Order = Mapper.Map<PaymentRequestOrderModel>(order);
                model.Transactions = Mapper.Map<List<PaymentRequestTransactionModel>>(transactions);

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
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
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
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(CreateAsync), model.ToJson(), exception);
                throw;
            }
        }

        /// <summary>
        /// Creates an order if it does not exist or expired and returns payment request details.
        /// </summary>
        /// <returns>The payment request details.</returns>
        /// <response code="200">The payment request details.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestsCreate")]
        [ProducesResponseType(typeof(PaymentRequestDetailsModel), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> ChechoutAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                IPaymentRequest paymentRequest =
                    await _paymentRequestService.CheckoutAsync(merchantId, paymentRequestId);

                IOrder order = await _orderService.GetAsync(paymentRequestId, paymentRequest.OrderId);

                IReadOnlyList<IPaymentRequestTransaction> transactions =
                    (await _transactionsService.GetAsync(paymentRequest.WalletAddress)).ToList();

                var model = Mapper.Map<PaymentRequestDetailsModel>(paymentRequest);
                model.Order = Mapper.Map<PaymentRequestOrderModel>(order);
                model.Transactions = Mapper.Map<List<PaymentRequestTransactionModel>>(transactions);

                return Ok(model);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(ChechoutAsync),
                    new
                    {
                        MerchantId = merchantId,
                        PaymentRequestId = paymentRequestId
                    }.ToJson(), exception);
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
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ValidateModel]
        public async Task<IActionResult> RefundAsync([FromBody] RefundRequestModel request)
        {
            try
            {
                RefundResult refundResult = await _refundService.ExecuteAsync(request.MerchantId,
                    request.PaymentRequestId, request.DestinationAddress);

                return Ok(Mapper.Map<RefundResponseModel>(refundResult));
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(PaymentRequestsController), nameof(RefundAsync), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
