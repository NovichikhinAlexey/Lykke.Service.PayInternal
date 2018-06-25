using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Transactions.Ethereum;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/ethereumTransactions")]
    public class EthereumTransactionsController : Controller
    {
        [HttpPost]
        [Route("inbound")]
        [SwaggerOperation(nameof(RegisterInboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> RegisterInboundTransaction(
            [FromBody] RegisterInboundTxRequest request)
        {
            return Ok();
        }

        [HttpPost]
        [Route("outbound")]
        [SwaggerOperation(nameof(RegisterOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> RegisterOutboundTransaction(
            [FromBody] RegisterOutboundTxRequest request)
        {
            return Ok();
        }

        [HttpPost]
        [Route("outbound/complete")]
        [SwaggerOperation(nameof(CompleteOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CompleteOutboundTransaction([FromBody] CompleteOutboundTxRequest request)
        {
            return NoContent();
        }

        [HttpPost]
        [Route("outbound/fail")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] FailOutboundTxRequest request)
        {
            return NoContent();
        }
    }
}
