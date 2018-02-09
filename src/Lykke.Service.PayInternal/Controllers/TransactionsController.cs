using System;
using System.Net;
using System.Threading.Tasks;
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
    }
}
