using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class BtcTransferController : Controller
    {
        private readonly IBtcTransferService _btcTransferService;
        private readonly ILog _log;

        public BtcTransferController(IBtcTransferService btcTransferService, ILog log)
        {
            _btcTransferService = btcTransferService ?? throw new ArgumentNullException(nameof(btcTransferService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Executes BTC transfer from source addresses to destination address
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of transaction ids</returns>
        [HttpPost]
        [SwaggerOperation("BtcFreeTransfer")]
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BtcFreeTransfer([FromBody] BtcFreeTransferRequest request)
        {
            try
            {
                string transactionId = await _btcTransferService.Execute(request.ToDomain());

                return Ok(new BtcTransferResponse {TransactionId = transactionId});
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(BtcTransferController), nameof(BtcFreeTransfer), ex);

                if (ex is BtcTransferException btcException)
                    return StatusCode((int) HttpStatusCode.InternalServerError, ErrorResponse.Create(btcException.Message));
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
