using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Transfers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class TransfersController : Controller
    {
        private readonly IBtcTransferService _btcTransferService;
        private readonly ILog _log;

        public TransfersController(IBtcTransferService btcTransferService, ILog log)
        {
            _btcTransferService = btcTransferService ?? throw new ArgumentNullException(nameof(btcTransferService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Executes BTC multipartTransfer from source addresses to destination address
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of transaction ids</returns>
        [HttpPost]
        [Route("transfers/BtcFreeTransfer")]
        [SwaggerOperation("BtcFreeTransfer")]
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        [ValidateModel]
        public async Task<IActionResult> BtcFreeTransferAsync([FromBody] BtcFreeTransferRequest request)
        {
            try
            {
                string transactionId = await _btcTransferService.ExecuteAsync(Mapper.Map<BtcTransfer>(request));

                return Ok(new BtcTransferResponse {TransactionId = transactionId});
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransfersController), nameof(BtcFreeTransferAsync), ex);

                if (ex is TransferException btcException)
                    return StatusCode((int) HttpStatusCode.InternalServerError, ErrorResponse.Create(btcException.Message));
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
