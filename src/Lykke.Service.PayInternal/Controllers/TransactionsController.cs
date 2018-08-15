using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Transactions;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.PayInternal.Core;
using LykkePay.Common.Validation;

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
            ILogFactory logFactory,
            ITransactionsManager transactionsManager)
        {
            _paymentRequestService = paymentRequestService;
            _transactionsService = transactionsService;
            _log = logFactory.CreateLog(this);
            _transactionsManager = transactionsManager;
        }

        [HttpPost]
        [Route("payment/lykke")]
        [SwaggerOperation(nameof(CreateLykkePaymentTransacton))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreateLykkePaymentTransacton([FromBody] CreateLykkeTransactionRequest request)
        {
            try
            {
                var command = Mapper.Map<CreateLykkeTransactionCommand>(request,
                    opts => opts.Items["TransactionType"] = TransactionType.Payment);

                await _transactionsManager.CreateLykkeTransactionAsync(command);

                _log.Info("Create new lykke transaction command", command);

                return Ok();
            }
            catch (LykkeOperationOrderNotFoundException e)
            {
                _log.Error(e, new {e.OperationId});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.WalletAddress,
                    e.PaymentRequestId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
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

                _log.Info("Create new transaction command", command.ToJson());

                return Ok();
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.WalletAddress,
                    e.PaymentRequestId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedAssetException e)
            {
                _log.Error(e, new {e.AssetId,});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (BlockchainWalletNotLinkedException e)
            {
                _log.Error(e, new
                {
                    e.Blockchain,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
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

                _log.Info("Update transaction command", command.ToJson());

                return Ok();
            }
            catch (TransactionNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.Blockchain,
                    e.IdentityType,
                    e.Identity,
					e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.Error(e, new
                {
                    e.MerchantId,
                    e.WalletAddress,
                    e.PaymentRequestId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (BlockchainWalletNotLinkedException e)
            {
                _log.Error(e, new
                {
                    e.Blockchain,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
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
            var response = await _transactionsService.GetNotExpiredAsync();

            return Ok(Mapper.Map<List<TransactionStateResponse>>(response));
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
            IEnumerable<IPaymentRequestTransaction> txs =
                await _transactionsService.GetByBcnIdentityAsync(request.Blockchain, request.IdentityType, request.Identity);

            foreach (IPaymentRequestTransaction tx in txs)
            {
                await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
            }

            return Ok();
        }
    }
}
