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
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Transactions;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ITransactionsService _transactionsService;
        private readonly ITransactionsManager _transactionsManager;
        private readonly ILog _log;

        public TransactionsController(
            ITransactionsService transactionsService,
            IPaymentRequestService paymentRequestService,
            ILog log,
            ITransactionsManager transactionsManager)
        {
            _paymentRequestService = paymentRequestService;
            _transactionsService = transactionsService;
            _log = log;
            _transactionsManager = transactionsManager;
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
        [ValidateModel]
        public async Task<IActionResult> CreatePaymentTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var command = Mapper.Map<CreateTransactionCommand>(request,
                    opts => opts.Items["TransactionType"] = TransactionType.Payment);

                await _transactionsManager.CreateTransactionAsync(command);

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
            catch (BlockchainWalletNotLinkedException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreatePaymentTransaction), new
                {
                    ex.Blockchain,
                    ex.WalletAddress
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreatePaymentTransaction), ex);

                throw;
            }
        }
        /// <summary>
        /// Return payment source wallets
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{paymentRequestId}")]
        [SwaggerOperation(nameof(PaymentTransaction))]
        [ProducesResponseType(typeof(IReadOnlyList<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PaymentTransaction(string paymentRequestId)
        {
            try
            {
                var transactions = await _transactionsService.GetTransactionsByPaymentRequestAsync(paymentRequestId);
                var addresses = new List<string>();
                if (transactions != null)
                {
                    foreach (var transaction in transactions)
                        addresses.AddRange(transaction.SourceWalletAddresses);
                }
                return Ok(addresses);
            }
            catch (PaymentRequestNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(PaymentTransaction), new
                {
                    ex.MerchantId,
                    ex.WalletAddress,
                    ex.PaymentRequestId
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(PaymentTransaction), ex);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
        /// <summary>
        /// Updates existing transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("UpdateTransaction")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            try
            {
                var command = Mapper.Map<UpdateTransactionCommand>(request);

                await _transactionsManager.UpdateTransactionAsync(command);

                return Ok();
            }
            catch (TransactionNotFoundException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), new
                {
                    ex.Blockchain,
                    ex.IdentityType,
                    ex.Identity,
					ex.WalletAddress
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
            catch (BlockchainWalletNotLinkedException ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), new
                {
                    ex.Blockchain,
                    ex.WalletAddress
                }.ToJson(), ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), ex);

                throw;
            }
        }

        /// <summary>
        /// Finds and returns all monitored (i.e., not expired and not fully confirmed yet) transactions.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllMonitored")]
        [SwaggerOperation("GetAllMonitored")]
        [ProducesResponseType(typeof(List<TransactionStateResponse>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllMonitoredAsync()
        {
            try
            {
                var response = await _transactionsService.GetNotExpiredAsync();

                return Ok(Mapper.Map<List<TransactionStateResponse>>(response));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(GetAllMonitoredAsync), ex);

                throw;
            }
        }


        /// <summary>
        /// Notifies about transaction expiration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("expired")]
        [SwaggerOperation("SetExpired")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> Expired([FromBody] TransactionExpiredRequest request)
        {
            try
            {
                IEnumerable<IPaymentRequestTransaction> txs =
                    await _transactionsService.GetByBcnIdentityAsync(request.Blockchain, request.IdentityType, request.Identity);

                foreach (IPaymentRequestTransaction tx in txs)
                {
                    await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(Expired), ex);

                throw;
            }
        }
    }
}
