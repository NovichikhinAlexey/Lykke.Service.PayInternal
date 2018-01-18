using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly IMerchantOrdersService _merchantOrdersService;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILog _log;

        public OrdersController(
            IMerchantOrdersService merchantOrdersService,
            IAssetsLocalCache assetsLocalCache,
            ILog log)
        {
            _merchantOrdersService =
                merchantOrdersService ?? throw new ArgumentNullException(nameof(merchantOrdersService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates new order for the invoice
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreateOrder")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(CreateOrderResponse), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (await _assetsLocalCache.GetAssetByIdAsync(request.InvoiceAssetId) == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.InvoiceAssetId)} doesn't exist'"));
            }

            if (await _assetsLocalCache.GetAssetByIdAsync(request.ExchangeAssetId) == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.ExchangeAssetId)} doesn't exist'"));
            }

            if (await _assetsLocalCache.GetAssetPairByIdAsync(request.AssetPairId) == null)
            {
                return BadRequest(ErrorResponse.Create($"{nameof(request.AssetPairId)} doesn't exist'"));
            }

            try
            {
                var newOrder = await _merchantOrdersService.CreateOrder(request.ToDomain());

                return Ok(newOrder.ToApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(OrdersController), nameof(CreateOrder), request.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Recreates order with specified wallet address if the previous one is out of date. If not then returns it.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("recreate")]
        [SwaggerOperation("RecreateOrder")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(CreateOrderResponse), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> ReCreateOrder([FromBody] ReCreateOrderRequest request)
        {
            try
            {
                var order = await _merchantOrdersService.ReCreateOrder(request.ToDomain());

                return Ok(order.ToApiModel());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(OrdersController), nameof(ReCreateOrder), request.ToJson(), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
