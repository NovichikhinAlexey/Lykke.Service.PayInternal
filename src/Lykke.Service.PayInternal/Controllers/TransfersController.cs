using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Transfers;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class TransfersController : Controller
    {
        private readonly IBtcTransferService _btcTransferService;
        private readonly ILog _log;

        public TransfersController(
            [NotNull] IBtcTransferService btcTransferService, 
            [NotNull] ILogFactory logFactory)
        {
            _btcTransferService = btcTransferService ?? throw new ArgumentNullException(nameof(btcTransferService));
            _log = logFactory.CreateLog(this);
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
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> BtcFreeTransferAsync([FromBody] BtcFreeTransferRequest request)
        {
            try
            {
                string transactionId = await _btcTransferService.ExecuteAsync(Mapper.Map<BtcTransfer>(request));

                return Ok(new BtcTransferResponse {TransactionId = transactionId});
            }
            catch (TransferException e)
            {
                _log.ErrorWithDetails(e, new {e.Code});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
