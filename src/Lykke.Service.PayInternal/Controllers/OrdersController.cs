using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class OrdersController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IOrderService _orderService;
        private readonly IMerchantService _merchantService;
        private readonly ILog _log;

        public OrdersController(
            [NotNull] IPaymentRequestService paymentRequestService,
            [NotNull] IOrderService orderService,
            [NotNull] ILogFactory logFactory, 
            [NotNull] IMerchantService merchantService)
        {
            _paymentRequestService = paymentRequestService;
            _orderService = orderService;
            _merchantService = merchantService;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Returns an order by id.
        /// </summary>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <param name="orderId">The order id.</param>
        /// <returns>The payment request order.</returns>
        /// <response code="200">The payment request order.</response>
        [HttpGet]
        [Route("merchants/paymentrequests/{paymentRequestId}/orders/{orderId}")]
        [SwaggerOperation("OrdersGetByPaymentRequestId")]
        [ProducesResponseType(typeof(OrderModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(string paymentRequestId, string orderId)
        {
            IOrder order = await _orderService.GetAsync(paymentRequestId, orderId);

            if (order == null)
                return NotFound();

            var model = Mapper.Map<OrderModel>(order);

            return Ok(model);
        }

        /// <summary>
        /// Creates an order if it does not exist or expired.
        /// </summary>
        /// <param name="model">The order creation information.</param>
        /// <returns>An active order related with payment request.</returns>
        /// <response code="200">An active order related with payment request.</response>
        [HttpPost]
        [Route("orders")]
        [SwaggerOperation("OrdersChechout")]
        [ProducesResponseType(typeof(OrderModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> ChechoutAsync([FromBody] ChechoutRequestModel model)
        {
            IMerchant merchant = await _merchantService.GetAsync(model.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            try
            {
                IPaymentRequest paymentRequest =
                    await _paymentRequestService.CheckoutAsync(model.MerchantId, model.PaymentRequestId, model.Force);

                IOrder order = await _orderService.GetAsync(paymentRequest.Id, paymentRequest.OrderId);

                return Ok(Mapper.Map<OrderModel>(order));
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, new {e.Asset});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, new {e.AssetId});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MarkupNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetPairId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Creates an order if it does not exist or expired.
        /// </summary>
        /// <param name="model">The order creation information.</param>
        /// <returns>An active order related with payment request.</returns>
        /// <response code="200">An active order related with payment request.</response>
        [HttpPost]
        [Route("orders/calculate")]
        [SwaggerOperation("GetCalculatedAmountInfo")]
        [ProducesResponseType(typeof(ICalculatedAmountInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> GetCalculatedAmountInfoAsync([FromBody] GetCalculatedAmountInfoRequest model)
        {
            try
            {
                ICalculatedAmountInfo calculatedAmountInfo = await _orderService.GetCalculatedAmountInfoAsync(
                    model.SettlementAssetId,
                    model.PaymentAssetId,
                    model.Amount,
                    model.MerchantId);

                return Ok(calculatedAmountInfo);
            }
            catch (MerchantNotFoundException e)
            {
                _log.Error(e, new {e.MerchantId});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, new {e.Asset});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MarkupNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.AssetPairId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MarketPriceZeroException e)
            {
                _log.Error(e, new {e.PriceType});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedAssetPairPriceMethodException e)
            {
                _log.Error(e, new {e.PriceMethod});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
