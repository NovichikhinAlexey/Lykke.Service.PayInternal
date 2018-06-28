﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayHistory.Client.Publisher;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Transactions.Ethereum;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/ethereumTransactions")]
    public class EthereumTransactionsController : Controller
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly HistoryOperationPublisher _historyOperationPublisher;
        private readonly IMerchantWalletService _merchantWalletService;
        private readonly ILog _log;

        public EthereumTransactionsController(
            [NotNull] ITransactionsService transactionsService, 
            [NotNull] IPaymentRequestService paymentRequestService, 
            [NotNull] ILog log, 
            [NotNull] HistoryOperationPublisher historyOperationPublisher, 
            [NotNull] IMerchantWalletService merchantWalletService)
        {
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _historyOperationPublisher = historyOperationPublisher ?? throw new ArgumentNullException(nameof(historyOperationPublisher));
            _merchantWalletService = merchantWalletService ?? throw new ArgumentNullException(nameof(merchantWalletService));
        }

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
                var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    request.Blockchain,
                    request.IdentityType,
                    request.Identity)).ToList();

                if (!txs.Any())
                {
                    switch (request.WorkflowType)
                    {
                        // payment
                        case WorkflowType.LykkePay:
                            //todo: move 3 to settings
                            var cmd = Mapper.Map<CreateTransactionCommand>(request,
                                opt => opt.Items["Confirmations"] = 3);
                            await _transactionsService.CreateTransactionAsync(cmd);
                            await _paymentRequestService.UpdateStatusAsync(cmd.WalletAddress);
                            break;
                        // cashin
                        case WorkflowType.Airlines:
                            await _transactionsService.CreateTransactionAsync(new CreateTransactionCommand
                            {
                                Amount = request.Amount,
                                Blockchain = request.Blockchain,
                                AssetId = request.AssetId,
                                BlockId = request.BlockId,
                                //todo: move 3 to settings
                                Confirmations = 3,
                                FirstSeen = request.FirstSeen,
                                Hash = request.Hash,
                                Identity = request.Identity,
                                IdentityType = request.IdentityType,
                                SourceWalletAddresses = new[] {request.FromAddress},
                                Type = TransactionType.CashIn
                            });

                            IMerchantWallet merchantWallet =
                                await _merchantWalletService.GetByAddressAsync(request.Blockchain, request.ToAddress);

                            await _historyOperationPublisher.PublishAsync(new HistoryOperation
                            {
                                Amount = request.Amount,
                                AssetId = request.AssetId,
                                Type = HistoryOperationType.Recharge,
                                CreatedOn = DateTime.UtcNow,
                                TxHash = request.Hash,
                                MerchantId = merchantWallet?.MerchantId,
                            });
                            break;
                        default: throw new UnexpectedWorkflowTypeException(request.WorkflowType);
                    }

                    return Ok();
                }

                foreach (var tx in txs)
                {
                    switch (tx.TransactionType)
                    {
                        case TransactionType.Payment:
                            // todo: move 3 to settings
                            await _transactionsService.UpdateAsync(
                                Mapper.Map<UpdateTransactionCommand>(request, opt => opt.Items["Confirmations"] = 3));
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                            break;
                        case TransactionType.Exchange:
                            // todo: move 3 to settings
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = request.Amount,
                                BlockId = request.BlockId,
                                Confirmations = 3,
                                FirstSeen = request.FirstSeen,
                                Hash = request.Hash,
                                Identity = request.Identity,
                                IdentityType = request.IdentityType
                            });

                            IMerchantWallet merchantWallet =
                                await _merchantWalletService.GetByAddressAsync(request.Blockchain, request.ToAddress);

                            await _historyOperationPublisher.PublishAsync(new HistoryOperation
                            {
                                Amount = request.Amount,
                                AssetId = request.AssetId,
                                Type = HistoryOperationType.IncomingExchange,
                                CreatedOn = DateTime.UtcNow,
                                TxHash = request.Hash,
                                MerchantId = merchantWallet?.MerchantId,
                            });
                            break;
                        case TransactionType.Settlement:
                            // todo: move 3 to settings
                            // todo: for settlement better to map walletaddres because we know it (it is FromAddress)
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = request.Amount,
                                BlockId = request.BlockId,
                                Confirmations = 3,
                                FirstSeen = request.FirstSeen,
                                Hash = request.Hash,
                                Identity = request.Identity,
                                IdentityType = request.IdentityType
                            });
                            break;
                        default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                    }
                }

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

        [HttpPost]
        [Route("outbound")]
        [SwaggerOperation(nameof(RegisterOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> RegisterOutboundTransaction(
            [FromBody] RegisterOutboundTxRequest request)
        {
            try
            {
                var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    request.Blockchain,
                    request.IdentityType,
                    request.Identity)).ToList();

                if (!txs.Any())
                    throw new OutboundTransactionsNotFound(request.Blockchain, request.IdentityType, request.Identity);

                foreach (var tx in txs)
                {
                    switch (tx.TransactionType)
                    {
                        case TransactionType.Payment:
                            // todo: move 0 to settings
                            await _transactionsService.UpdateAsync(
                                Mapper.Map<UpdateTransactionCommand>(request, opt => opt.Items["Confirmations"] = 0));
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                            break;
                        case TransactionType.Refund:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = request.Amount,
                                WalletAddress = tx.WalletAddress,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 0,
                                Hash = request.Hash,
                                FirstSeen = request.FirstSeen,
                                BlockId = request.BlockId
                            });
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                            break;
                        case TransactionType.Exchange:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = request.Amount,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 0,
                                BlockId = request.BlockId,
                                FirstSeen = request.FirstSeen,
                                Hash = request.Hash
                            });
                            break;
                        case TransactionType.Settlement:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = request.Amount,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 0,
                                BlockId = request.BlockId,
                                FirstSeen = request.FirstSeen,
                                Hash = request.Hash
                            });
                            break;
                        default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                    }
                }

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

        [HttpPost]
        [Route("outbound/complete")]
        [SwaggerOperation(nameof(CompleteOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CompleteOutboundTransaction([FromBody] CompleteOutboundTxRequest request)
        {
            try
            {
                var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    request.Blockchain,
                    request.IdentityType,
                    request.Identity)).ToList();

                if (!txs.Any())
                    throw new OutboundTransactionsNotFound(request.Blockchain, request.IdentityType, request.Identity);

                foreach (var tx in txs)
                {
                    switch (tx.TransactionType)
                    {
                        case TransactionType.Payment:
                            // todo: move 3 to settings
                            await _transactionsService.UpdateAsync(
                                Mapper.Map<UpdateTransactionCommand>(request, opt => opt.Items["Confirmations"] = 3));
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                            break;
                        case TransactionType.Refund:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = tx.Amount,
                                WalletAddress = tx.WalletAddress,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 3,
                                Hash = tx.TransactionId,
                                FirstSeen = tx.FirstSeen,
                                BlockId = tx.BlockId
                            });
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                            break;
                        case TransactionType.Exchange:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = tx.Amount,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 3,
                                BlockId = tx.BlockId,
                                FirstSeen = tx.FirstSeen,
                                Hash = tx.TransactionId
                            });

                            IMerchantWallet merchantWallet =
                                await _merchantWalletService.GetByAddressAsync(request.Blockchain, request.FromAddress);

                            await _historyOperationPublisher.PublishAsync(new HistoryOperation
                            {
                                Amount = request.Amount,
                                AssetId = tx.AssetId,
                                Type = HistoryOperationType.OutgoingExchange,
                                CreatedOn = DateTime.UtcNow,
                                TxHash = request.Hash,
                                MerchantId = merchantWallet?.MerchantId,
                            });
                            break;
                        case TransactionType.Settlement:
                            await _transactionsService.UpdateAsync(new UpdateTransactionCommand
                            {
                                Blockchain = request.Blockchain,
                                Amount = tx.Amount,
                                IdentityType = request.IdentityType,
                                Identity = request.Identity,
                                Confirmations = 3,
                                BlockId = tx.BlockId,
                                FirstSeen = tx.FirstSeen,
                                Hash = tx.TransactionId
                            });
                            break;
                        default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                    }
                }

                return NoContent();
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
                _log.WriteError(nameof(CompleteOutboundTransaction), new { e.TransactionType }, e);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        [HttpPost]
        [Route("outbound/fail")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] FailOutboundTxRequest request)
        {
            // todo:
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("outbound/notEnoughFunds")]
        [SwaggerOperation(nameof(FailOutboundTransaction))]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> FailOutboundTransaction([FromBody] NotEnoughFundsOutboundTxRequest request)
        {
            try
            {
                var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    request.Blockchain,
                    request.IdentityType,
                    request.Identity)).ToList();

                if (!txs.Any())
                    throw new OutboundTransactionsNotFound(request.Blockchain, request.IdentityType, request.Identity);

                foreach (var tx in txs)
                {
                    switch (tx.TransactionType)
                    {
                        case TransactionType.Payment:
                            await _transactionsService.UpdateAsync(
                                Mapper.Map<UpdateTransactionCommand>(request, opt => opt.Items["Confirmations"] = 3));
                            await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress,
                                PaymentRequestStatusInfo.New());
                            break;
                        default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                    }
                }

                return NoContent();
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
