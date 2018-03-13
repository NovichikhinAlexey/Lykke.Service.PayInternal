using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Refund;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Refunds;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using RefundResponse = Lykke.Service.PayInternal.Models.Refunds.RefundResponse;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class RefundsController : Controller
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly IRefundService _refundService;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public RefundsController(IRefundService refundService, ILog log)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates a new refund request for the specified payment request and (optionally) wallet address.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("refund")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(RefundResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        [ValidateModel]
        public async Task<IActionResult> CreateRefundRequestAsync([FromBody] RefundRequestModel request)
        {
            try
            {
                IRefund refund = await _refundService.ExecuteAsync(request);

                return Ok(refund.ToApiModel());
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(RefundsController), nameof(CreateRefundRequestAsync), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Gets the current state of the specified refund request.
        /// </summary>
        /// <param name="refundId">The refund request ID.</param>
        /// <param name="merchantId">Merchant id that owns the refund</param>
        /// <returns></returns>
        [HttpGet]
        [Route("refund/{merchantId}/{refundId}")]
        [SwaggerOperation("GetRefund")]
        [ProducesResponseType(typeof(RefundResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRefundAsync(string merchantId, string refundId)
        {
            if (string.IsNullOrWhiteSpace(refundId) ||
                string.IsNullOrWhiteSpace(refundId))
                return BadRequest(ErrorResponse.Create("Refund request ID can not be null."));

            var result = await _refundService.GetStateAsync(merchantId, refundId);

            if (result == null)
                return NotFound("The refund operation with the requested parameters does not exist.");

            return Ok(new RefundResponse
            {
                Amount = result.Amount,
                MerchantId = result.MerchantId,
                PaymentRequestId = result.PaymentRequestId,
                RefundId = result.RefundId,
                SettlementId = result.SettlementId
            });
        }
    }
}
