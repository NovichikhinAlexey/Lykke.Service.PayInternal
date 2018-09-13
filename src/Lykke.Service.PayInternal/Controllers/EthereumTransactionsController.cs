using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Models.Transactions.Ethereum;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/ethereumTransactions")]
    public class EthereumTransactionsController : Controller
    {
        private readonly IEthereumTransactionsManager _ethTransactionsManager;
        private readonly EthereumBlockchainSettings _ethereumSettings;
        private readonly ILog _log;

        public EthereumTransactionsController(
            [NotNull] IEthereumTransactionsManager ethTransactionsManager,
            [NotNull] ILogFactory logFactory, 
            [NotNull] EthereumBlockchainSettings ethereumSettings)
        {
            _ethTransactionsManager =
                ethTransactionsManager ?? throw new ArgumentNullException(nameof(ethTransactionsManager));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _log = logFactory.CreateLog(this);
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
        [ValidateModel]
        public async Task<IActionResult> RegisterInboundTransaction(
            [FromBody] RegisterInboundTxRequest request)
        {
            try
            {
                await _ethTransactionsManager.RegisterInboundAsync(Mapper.Map<RegisterInTxCommand>(request));

                return Ok();
            }
            catch (MerchantWalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.Network,
                    e.WalletAddress
                }.ToDetails());

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.WalletAddress,
                    e.MerchantId,
                    e.PaymentRequestId
                }.ToDetails());

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedWorkflowTypeException e)
            {
                _log.ErrorWithDetails(e, $"WorkflowType = {e.WorkflowType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.ErrorWithDetails(e, $"TransactionType ={e.TransactionType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (WalletAddressProcessingHostException e)
            {
                ConditionalLog(e, request.ToDetails());

                return Ok();
            }
        }

        /// <summary>
        /// Updates outgoing ethereum transaction
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been successfully updated</response>
        /// <response code="400">Transaction not found or unexpected transaction type</response>
        /// <response code="404">Transaction not found</response>
        [HttpPost]
        [Route("outbound")]
        [SwaggerOperation(nameof(RegisterOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> RegisterOutboundTransaction(
            [FromBody] RegisterOutboundTxRequest request)
        {
            try
            {
                await _ethTransactionsManager.UpdateOutgoingAsync(Mapper.Map<UpdateOutTxCommand>(request));

                return Ok();
            }
            catch (OutboundTransactionsNotFound e)
            {
                ConditionalLog(e, request.ToDetails());

                return Ok();
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.ErrorWithDetails(e, $"TransactionType = {e.TransactionType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Completes outgoing ethereum transaction
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been successfully completed</response>
        /// <response code="400">Unexpected transaction type, merchant wallet not found, insufficient funds</response>
        /// <response code="404">Transaction not found</response>
        [HttpPost]
        [Route("outbound/complete")]
        [SwaggerOperation(nameof(CompleteOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> CompleteOutboundTransaction([FromBody] CompleteOutboundTxRequest request)
        {
            try
            {
                await _ethTransactionsManager.CompleteOutgoingAsync(
                    Mapper.Map<CompleteOutTxCommand>(request));

                return Ok();
            }
            catch (MerchantWalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.Network,
                    e.WalletAddress
                }.ToDetails());

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (OutboundTransactionsNotFound e)
            {
                ConditionalLog(e, request.ToDetails());

                return Ok();
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.ErrorWithDetails(e, $"TransactionType = {e.TransactionType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InsufficientFundsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.AssetId,
                    e.WalletAddress
                }.ToDetails());

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Fails outgoing ethereum transaction (not defined reason)
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been failed</response>
        /// <response code="400">Unexpected transaction type</response>
        /// <response code="404">Transaction not found</response>
        [HttpPost]
        [Route("outbound/fail")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] FailOutboundTxRequest request)
        {
            try
            {
                await _ethTransactionsManager.FailOutgoingAsync(Mapper.Map<FailOutTxCommand>(request));

                return Ok();
            }
            catch (OutboundTransactionsNotFound e)
            {
                ConditionalLog(e, request.ToDetails());

                return Ok();
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.ErrorWithDetails(e, $"TransactionType = {e.TransactionType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Fails outgoing ethereum transaction (not enough funds reason)
        /// </summary>
        /// <param name="request">Outgoing ethereum transaction details</param>
        /// <response code="200">Transaction has been failed</response>
        /// <response code="400">Unexpected transaction type</response>
        /// <response code="404">Transaction not found</response>
        [HttpPost]
        [Route("outbound/notEnoughFunds")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] NotEnoughFundsOutboundTxRequest request)
        {
            try
            {
                await _ethTransactionsManager.FailOutgoingAsync(
                    Mapper.Map<NotEnoughFundsOutTxCommand>(request));

                return Ok();
            }
            catch (OutboundTransactionsNotFound e)
            {
                ConditionalLog(e, request.ToDetails());

                return Ok();
            }
            catch (UnexpectedTransactionTypeException e)
            {
                _log.ErrorWithDetails("NotEnoughFundsOutboundTx", e, $"TransactoinType = {e.TransactionType.ToString()}");

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        private void ConditionalLog(Exception e, object context)
        {
            switch (_ethereumSettings.EventsProcessingHostMismatchLevel)
            {
                case LogLevel.Info:
                    _log.Info(e.Message, context);
                    break;
                case LogLevel.Warning:
                    _log.Warning(e.Message, context: context);
                    break;
                case LogLevel.Error:
                    _log.Error(e, context: context);
                    break;
            }
        }
    }
}
