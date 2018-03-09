using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ITransactionsService _transactionsService;
        private readonly ILog _log;

        public TransactionsController(
            ITransactionsService transactionsService,
            IPaymentRequestService paymentRequestService,
            ILog log)
        {
            _paymentRequestService = paymentRequestService;
            _transactionsService = transactionsService;
            _log = log;
        }

        /// <summary>
        /// Registers new transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreateTransaction")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                await _transactionsService.Create(request.ToDomain());

                await _paymentRequestService.ProcessAsync(request.WalletAddress);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreateTransaction), ex);

                if (ex is PaymentRequestNotFoundException || ex is UnexpectedAssetException)
                {
                    return BadRequest(ErrorResponse.Create(ex.Message));
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Updates existing transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("UpdateTransaction")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                await _transactionsService.Update(request.ToDomain());

                await _paymentRequestService.ProcessAsync(request.WalletAddress);

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), ex);

                if (ex is TransactionNotFoundException || ex is PaymentRequestNotFoundException)
                {
                    return BadRequest(ErrorResponse.Create(ex.Message));
                }
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Finds and returns all monitored (i.e., not expired and not fully confirmed yet) transactions.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllMonitored")]
        [SwaggerOperation("GetAllMonitored")]
        [ProducesResponseType(typeof(List<TransactionStateResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllMonitoredAsync()
        {
            try
            {
                var response = await _transactionsService.GetAllMonitoredAsync();
                if (!response.Any())
                    return NotFound(ErrorResponse.Create("There are no monitored transactions right now."));

                return Ok(Mapper.Map<List<TransactionStateResponse>>(response));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(GetAllMonitoredAsync), ex);
                return BadRequest((ErrorResponse.Create(ex.Message)));
            }
        }
    }
}
