using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using UpdateRefundEthOutgoingTxCommand = Lykke.Service.PayInternal.Services.Domain.UpdateRefundEthOutgoingTxCommand;

namespace Lykke.Service.PayInternal.Services
{
    public class TransactionsManager : ITransactionsManager
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IWalletHistoryService _walletHistoryService;
        private readonly int _transactionConfirmationCount;
        private readonly ILog _log;

        public TransactionsManager(
            [NotNull] ITransactionsService transactionsService,
            [NotNull] IPaymentRequestService paymentRequestService, 
            [NotNull] ILog log, 
            [NotNull] IWalletHistoryService walletHistoryService, 
            int transactionConfirmationCount)
        {
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _walletHistoryService = walletHistoryService ?? throw new ArgumentNullException(nameof(walletHistoryService));
            _transactionConfirmationCount = transactionConfirmationCount;
            _log = log.CreateComponentScope(nameof(TransactionsManager)) ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand command)
        {
            IPaymentRequestTransaction transaction = await _transactionsService.CreateTransactionAsync(command);

            await _paymentRequestService.UpdateStatusAsync(transaction.WalletAddress);

            return transaction;
        }

        public async Task<IPaymentRequestTransaction> CreateLykkeTransactionAsync(ICreateLykkeTransactionCommand command)
        {
            IPaymentRequestTransaction transaction = await _transactionsService.CreateLykkeTransactionAsync(command);

            await _paymentRequestService.UpdateStatusAsync(transaction.WalletAddress);

            return transaction;
        }

        public async Task RegisterEthInboundTxAsync(RegisterEthInboundTxCommand cmd)
        {
            _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "Started inbound tx registration");

            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    cmd.Blockchain,
                    cmd.IdentityType,
                    cmd.Identity)).ToList();

            if (!txs.Any())
            {
                switch (cmd.WorkflowType)
                {
                    // payment
                    case WorkflowType.LykkePay:
                        _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "Lykke Pay payment transaction");
                        var lykkePayCmd = Mapper.Map<CreateTransactionCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount);
                        await _transactionsService.CreateTransactionAsync(lykkePayCmd);
                        await _paymentRequestService.UpdateStatusAsync(lykkePayCmd.WalletAddress);
                        break;
                    // cashin
                    case WorkflowType.Airlines:
                        _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "IATA cashin transaction");
                        await _transactionsService.CreateTransactionAsync(
                            Mapper.Map<CreateCashInTransactionCommand>(cmd,
                                opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _walletHistoryService.PublishCashIn(Mapper.Map<WalletHistoryCommand>(cmd));
                        break;
                    default: throw new UnexpectedWorkflowTypeException(cmd.WorkflowType);
                }

                return;
            }

            foreach (var tx in txs)
            {
                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                        _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "Payment transaction update");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateTransactionCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Exchange:
                        _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "Exchange incoming transaction update");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateExchangeEthInboundTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _walletHistoryService.PublishIncomingExchange(Mapper.Map<WalletHistoryCommand>(cmd));
                        break;
                    case TransactionType.Settlement:
                        _log.WriteInfo(nameof(RegisterEthInboundTxAsync), cmd, "Settlement incoming transaction update");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateSettlementEthInboundTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        break;
                    default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                }
            }
        }

        public async Task UpdateEthOutgoingTxAsync(UpdateEthOutgoingTxCommand cmd)
        {
            _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Started outgoing tx registration");

            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                        _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Outgoing payment");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdatePaymentEthOutgoingTxCommand>(cmd));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Refund:
                        _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Outgoing payment refund");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateRefundEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["WalletAddress"] = tx.WalletAddress));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Exchange:
                        _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Outgoing exchange operation");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateExchangeEthOutgoingTxCommand>(cmd));
                        break;
                    case TransactionType.Settlement:
                        _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Outgoing settlement operation");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateSettlementEthOutgoingTxCommand>(cmd));
                        break;
                    case TransactionType.CashOut:
                        _log.WriteInfo(nameof(UpdateEthOutgoingTxAsync), cmd, "Cashout operation");
                        await _transactionsService.UpdateAsync(Mapper.Map<UpdateCashoutEthOutgoingTxCommand>(cmd));
                        break;
                    default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                }
            }
        }

        public async Task UpdateTransactionAsync(IUpdateTransactionCommand cmd)
        {
            await _transactionsService.UpdateAsync(cmd);

            if (string.IsNullOrEmpty(cmd.WalletAddress))
            {
                IEnumerable<IPaymentRequestTransaction> txs =
                    await _transactionsService.GetByBcnIdentityAsync(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

                foreach (IPaymentRequestTransaction tx in txs)
                {
                    await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                }
            }
            else
            {
                await _paymentRequestService.UpdateStatusAsync(cmd.WalletAddress);
            }
        }

        public async Task CompleteEthOutgoingTxAsync(CompleteEthOutgoingTxCommand cmd)
        {
            _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Started outgoing tx complete");

            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    cmd.Blockchain,
                    cmd.IdentityType,
                    cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                        _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Complete outgoing payment");
                        await _transactionsService.UpdateAsync(Mapper.Map<CompletePaymentEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Refund:
                        _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Complete outgoing refund");
                        await _transactionsService.UpdateAsync(Mapper.Map<CompleteRefundEthOutgoingTxCommand>(cmd,
                            opt =>
                            {
                                opt.Items["Confirmations"] = _transactionConfirmationCount;
                                opt.Items["WalletAddress"] = tx.WalletAddress;
                            }));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Exchange:
                        _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Complete outgoing exchange");
                        await _transactionsService.UpdateAsync(Mapper.Map<CompleteExchangeEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _walletHistoryService.PublishOutgoingExchange(
                            Mapper.Map<WalletHistoryCommand>(cmd, opt => opt.Items["AssetId"] = tx.AssetId));
                        break;
                    case TransactionType.Settlement:
                        _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Complete outgoing settlement");
                        await _transactionsService.UpdateAsync(Mapper.Map<CompleteSettlementEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        break;
                    case TransactionType.CashOut:
                        _log.WriteInfo(nameof(CompleteEthOutgoingTxAsync), cmd, "Complete outgoing cashout");
                        await _transactionsService.UpdateAsync(Mapper.Map<CompleteCashoutEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        break;
                    default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                }
            }
        }

        public async Task FailEthOutgoingTxAsync(NotEnoughFundsEthOutgoingTxCommand cmd)
        {
            _log.WriteInfo(nameof(FailEthOutgoingTxAsync), cmd, "Started outgoing tx fail");

            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                        _log.WriteInfo(nameof(FailEthOutgoingTxAsync), cmd, "Failing outgoing payment");
                        await _transactionsService.UpdateAsync(Mapper.Map<FailPaymentEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress,
                            PaymentRequestStatusInfo.New());
                        break;
                    case TransactionType.CashOut:
                        _log.WriteInfo(nameof(FailEthOutgoingTxAsync), cmd, "Failing cashout");
                        await _transactionsService.UpdateAsync(Mapper.Map<FailCashoutEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        break;
                    case TransactionType.Exchange:
                        _log.WriteInfo(nameof(FailEthOutgoingTxAsync), cmd, "Failing outgoing exchange");
                        await _transactionsService.UpdateAsync(Mapper.Map<FailExchangeEthOutgoingTxCommand>(cmd,
                            opt => opt.Items["Confirmations"] = _transactionConfirmationCount));
                        break;
                    default: throw new UnexpectedTransactionTypeException(tx.TransactionType);
                }
            }
        }
    }
}
