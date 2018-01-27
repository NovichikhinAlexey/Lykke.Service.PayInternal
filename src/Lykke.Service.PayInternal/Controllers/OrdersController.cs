using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILog _log;

        public OrdersController(
            IOrderService orderService,
            ILog log)
        {
            _orderService = orderService;
            _log = log;
        }

        /// <summary>
        /// Returns an acrive order if exist, otherwise creates new one.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request active order.</returns>
        /// <response code="200">The payment request active order.</response>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}/orders/active")]
        [SwaggerOperation("OrdersGetByPaymentRequestId")]
        [ProducesResponseType(typeof(OrderModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync(string merchantId, string paymentRequestId)
        {
            try
            {
                IOrder order = await _orderService.GetAsync(merchantId, paymentRequestId);

                var model = Mapper.Map<OrderModel>(order);

                return Ok(model);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(OrdersController), nameof(GetAsync),
                    new {PaymentRequestId = paymentRequestId}.ToJson(), exception);
                throw;
            }
        }
    }
}
