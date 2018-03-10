using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models.Refunds;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class RefundsController : Controller
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ITransferService _transferService;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public RefundsController(ITransferService transferService, ILog log)
        {
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
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
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateRefundRequestAsync([FromBody] RefundRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            await Task.Delay(50);

            return Ok(new RefundResponse());
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
            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(refundId))
                return BadRequest(ErrorResponse.Create("Refund request ID can not be null."));

            return Ok(new RefundResponse());
        }
    }
}
