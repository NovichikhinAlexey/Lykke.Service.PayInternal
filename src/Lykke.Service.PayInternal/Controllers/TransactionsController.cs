using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models.Transactions;
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
        /// Creates payment transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("payment")]
        [SwaggerOperation(nameof(CreatePaymentTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreatePaymentTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var domainRequest = request.ToDomain();
                domainRequest.Type = TransactionType.Payment;

                await _transactionsService.CreateTransaction(domainRequest);

                await _paymentRequestService.ProcessAsync(request.WalletAddress);

                return Ok();
            }
            catch (PaymentRequestNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreatePaymentTransaction), new
                {
                    ex.MerchantId,
                    ex.WalletAddress,
                    ex.PaymentRequestId
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (UnexpectedAssetException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreatePaymentTransaction), new
                {
                    ex.AssetId,
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreatePaymentTransaction), ex);
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

                if (!string.IsNullOrEmpty(request.WalletAddress))
                {
                    await _paymentRequestService.ProcessAsync(request.WalletAddress);
                }

                return Ok();
            }
            catch (TransactionNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), new
                {
                    ex.TransactionId
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (PaymentRequestNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), new
                {
                    ex.MerchantId,
                    ex.WalletAddress,
                    ex.PaymentRequestId
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), ex);
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
        [ProducesResponseType(typeof(List<TransactionStateResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllMonitoredAsync()
        {
            try
            {
                var response = await _transactionsService.GetAllMonitoredAsync();

                return Ok(Mapper.Map<List<TransactionStateResponse>>(response));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(GetAllMonitoredAsync), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
