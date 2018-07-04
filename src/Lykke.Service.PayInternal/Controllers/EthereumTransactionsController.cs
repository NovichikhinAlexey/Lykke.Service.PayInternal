using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayHistory.Client.Models;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Transactions.Ethereum;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/ethereumTransactions")]
    [ApiController]
    public class EthereumTransactionsController : ControllerBase
    {
        private readonly ITransactionsManager _transactionsManager;
        private readonly ILog _log;

        public EthereumTransactionsController(
            [NotNull] ITransactionsManager transactionsManager,
            [NotNull] ILog log)
        {
            _transactionsManager = transactionsManager ?? throw new ArgumentNullException(nameof(transactionsManager));
            _log = log.CreateComponentScope(nameof(EthereumTransactionsController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Registers or updates incoming ethereum transaction
        /// </summary>
        /// <param name="request">Incoming ethereum transaction details</param>
        /// <response code="200">Transaction has been successfully registered or updated</response>
        /// <response code="400">Mechant wallet, payment request not found, unexpected transaction or workflow type</response>
        [HttpPost]
        [Route("inbound")]
        [SwaggerOperation(nameof(RegisterInboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterInboundTransaction(
            [FromBody] RegisterInboundTxRequest request)
        {
            try
            {
                await _transactionsManager.RegisterEthInboundTxAsync(Mapper.Map<RegisterEthInboundTxCommand>(request));

                return Ok();
            }
            catch (PayHistoryApiException e)
            {
                _log.WriteError(nameof(RegisterInboundTransaction), request, e);

                return Ok();
            }
            catch (MerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(RegisterInboundTransaction), new
                {
                    e.MerchantId,
                    e.Network,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.WriteError(nameof(RegisterInboundTransaction), new
                {
                    e.WalletAddress,
                    e.MerchantId,
                    e.PaymentRequestId
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedWorkflowTypeException e)
            {
                _log.WriteError(nameof(RegisterInboundTransaction), new {e.WorkflowType}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.WriteError(nameof(RegisterInboundTransaction), new {e.TransactionType}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Updates outgoing ethereum transaction
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been successfully updated</response>
        /// <response code="400">Transaction not found or unexpected transaction type</response>
        [HttpPost]
        [Route("outbound")]
        [SwaggerOperation(nameof(RegisterOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterOutboundTransaction(
            [FromBody] RegisterOutboundTxRequest request)
        {
            try
            {
                await _transactionsManager.UpdateEthOutgoingTxAsync(Mapper.Map<UpdateEthOutgoingTxCommand>(request));

                return Ok();
            }
            catch (OutboundTransactionsNotFound e)
            {
                _log.WriteError(nameof(RegisterOutboundTransaction), new
                {
                    e.Blockchain,
                    e.Identity,
                    e.IdentityType
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.WriteError(nameof(RegisterOutboundTransaction), new {e.TransactionType}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Completes outgoing ethereum transaction
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been successfully completed</response>
        /// <response code="400">Transaction not found or unexpected transaction type</response>
        [HttpPost]
        [Route("outbound/complete")]
        [SwaggerOperation(nameof(CompleteOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CompleteOutboundTransaction([FromBody] CompleteOutboundTxRequest request)
        {
            try
            {
                await _transactionsManager.CompleteEthOutgoingTxAsync(
                    Mapper.Map<CompleteEthOutgoingTxCommand>(request));

                return Ok();
            }
            catch (PayHistoryApiException e)
            {
                _log.WriteError(nameof(CompleteOutboundTransaction), request, e);

                return Ok();
            }
            catch (MerchantWalletNotFoundException e)
            {
                _log.WriteError(nameof(CompleteOutboundTransaction), new
                {
                    e.MerchantId,
                    e.Network,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (OutboundTransactionsNotFound e)
            {
                _log.WriteError(nameof(CompleteOutboundTransaction), new
                {
                    e.Blockchain,
                    e.Identity,
                    e.IdentityType
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.WriteError(nameof(CompleteOutboundTransaction), new {e.TransactionType}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InsufficientFundsException e)
            {
                _log.WriteError(nameof(CompleteOutboundTransaction), new
                {
                    e.AssetId,
                    e.WalletAddress
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        [HttpPost]
        [Route("outbound/fail")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] FailOutboundTxRequest request)
        {
            // todo:
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fails outgoing ethereum transaction (not enough funds reason)
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been failed</response>
        /// <response code="400">Transaction not found or unexpected transaction type</response>
        [HttpPost]
        [Route("outbound/notEnoughFunds")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] NotEnoughFundsOutboundTxRequest request)
        {
            try
            {
                await _transactionsManager.FailEthOutgoingTxAsync(
                    Mapper.Map<NotEnoughFundsEthOutgoingTxCommand>(request));

                return Ok();
            }
            catch (OutboundTransactionsNotFound e)
            {
                _log.WriteError("NotEnoughFundsOutboundTx", new
                {
                    e.Blockchain,
                    e.Identity,
                    e.IdentityType
                }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.WriteError("NotEnoughFundsOutboundTx", new {e.TransactionType}, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
