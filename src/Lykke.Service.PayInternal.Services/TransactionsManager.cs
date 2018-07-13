using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Confirmations;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Context;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class TransactionsManager : ITransactionsManager
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IWalletHistoryService _walletHistoryService;
        private readonly IConfirmationsService _confirmationsService;
        private readonly int _transactionConfirmationCount;
        private readonly ILog _log;

        public TransactionsManager(
            [NotNull] ITransactionsService transactionsService,
            [NotNull] IPaymentRequestService paymentRequestService, 
            [NotNull] ILogFactory logFactory, 
            [NotNull] IWalletHistoryService walletHistoryService, 
            int transactionConfirmationCount, 
            [NotNull] IConfirmationsService confirmationsService)
        {
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _walletHistoryService = walletHistoryService ?? throw new ArgumentNullException(nameof(walletHistoryService));
            _transactionConfirmationCount = transactionConfirmationCount;
            _confirmationsService = confirmationsService ?? throw new ArgumentNullException(nameof(confirmationsService));
            _log = logFactory.CreateLog(this);
        }

        #region All transactions but Ethereum

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

        #endregion

        #region Only Ethereum transactions

        public async Task RegisterInboundAsync(RegisterInTxCommand cmd)
        {
            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                    cmd.Blockchain,
                    cmd.IdentityType,
                    cmd.Identity)).ToList();

            if (!txs.Any())
            {
                _log.Info($"Incoming transaction registration [workflow = {cmd.WorkflowType}]", cmd);

                ICreateTransactionCommand createCommand = MapToCreateCommand(cmd);

                await _transactionsService.CreateTransactionAsync(createCommand);

                switch (cmd.WorkflowType)
                {
                    case WorkflowType.LykkePay:
                        await _paymentRequestService.UpdateStatusAsync(createCommand.WalletAddress);
                        break;
                    case WorkflowType.Airlines:
                        await _walletHistoryService.PublishCashInAsync(Mapper.Map<WalletHistoryCommand>(cmd));
                        break;
                }

                return;
            }

            foreach (var tx in txs)
            {
                _log.Info($"Incoming transaction update [type={tx.TransactionType}]", cmd);

                IUpdateTransactionCommand updateCommand = MapToUpdateCommand(cmd, tx.TransactionType);

                await _transactionsService.UpdateAsync(updateCommand);

                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Exchange:
                        var context = tx.ContextData.DeserializeJson<ExchangeTransactonContext>();
                        if (context != null)
                            await _walletHistoryService.SetTxHashAsync(context.HistoryOperationId, cmd.Hash);
                        break;
                }
            }
        }

        public async Task UpdateOutgoingAsync(UpdateOutTxCommand cmd)
        {
            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                _log.Info($"Outgoing transaction update [type = {tx.TransactionType}]", cmd);

                IUpdateTransactionCommand updateCommand = MapToUpdateCommand(cmd, tx);

                await _transactionsService.UpdateAsync(updateCommand);

                if (tx.TransactionType == TransactionType.Payment || tx.TransactionType == TransactionType.Refund)
                    await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
            }
        }

        public async Task CompleteOutgoingAsync(CompleteOutTxCommand cmd)
        {
            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                _log.Info($"Complete outgoing transaction [type = {tx.TransactionType}]", cmd);

                IUpdateTransactionCommand updateCommand = MapToUpdateCommand(cmd, tx);

                await _transactionsService.UpdateAsync(updateCommand);

                switch (tx.TransactionType)
                {
                    case TransactionType.Payment:
                    case TransactionType.Refund:
                        await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress);
                        break;
                    case TransactionType.Exchange:
                        var exContext = tx.ContextData.DeserializeJson<ExchangeTransactonContext>();
                        if (exContext != null)
                            await _walletHistoryService.SetTxHashAsync(exContext.HistoryOperationId, updateCommand.Hash);
                        break;
                    case TransactionType.CashOut:
                        var cashoutContext = tx.ContextData.DeserializeJson<CashoutTransactionContext>();
                        if (cashoutContext != null)
                            await _walletHistoryService.SetTxHashAsync(cashoutContext.HistoryOperationId, updateCommand.Hash);
                        await _confirmationsService.ConfirmCashoutAsync(Mapper.Map<CashoutConfirmationCommand>(tx));
                        break;
                }
            }
        }

        public async Task FailOutgoingAsync(NotEnoughFundsOutTxCommand cmd)
        {
            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                _log.Info($"Failing outgoing transaction, not enough funds [type={tx.TransactionType}]", cmd);

                IUpdateTransactionCommand updateCommand = MapToUpdateCommand(cmd, tx.TransactionType);

                await _transactionsService.UpdateAsync(updateCommand);

                if (tx.TransactionType == TransactionType.Payment)
                    await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress, PaymentRequestStatusInfo.New());

                if (tx.TransactionType == TransactionType.Exchange)
                {
                    var context = tx.ContextData.DeserializeJson<ExchangeTransactonContext>();
                    if (context != null)
                        await _walletHistoryService.RemoveAsync(context.HistoryOperationId);
                }

                if (tx.TransactionType == TransactionType.CashOut)
                {
                    var context = tx.ContextData.DeserializeJson<CashoutTransactionContext>();
                    if (context != null)
                        await _walletHistoryService.RemoveAsync(context.HistoryOperationId);
                }
            }
        }

        public async Task FailOutgoingAsync(FailOutTxCommand cmd)
        {
            var txs = (await _transactionsService.GetByBcnIdentityAsync(
                cmd.Blockchain,
                cmd.IdentityType,
                cmd.Identity)).ToList();

            if (!txs.Any())
                throw new OutboundTransactionsNotFound(cmd.Blockchain, cmd.IdentityType, cmd.Identity);

            foreach (var tx in txs)
            {
                _log.Info($"Failing outgoing transaction [type={tx.TransactionType}]", cmd);

                IUpdateTransactionCommand updateCommand = MapToUpdateCommand(cmd, tx.TransactionType);

                await _transactionsService.UpdateAsync(updateCommand);

                if (tx.TransactionType == TransactionType.Payment)
                    await _paymentRequestService.UpdateStatusAsync(tx.WalletAddress,
                        PaymentRequestStatusInfo.Error(PaymentRequestProcessingError.UnknownPayment));

                if (tx.TransactionType == TransactionType.Exchange)
                {
                    var context = tx.ContextData.DeserializeJson<ExchangeTransactonContext>();
                    if (context != null)
                        await _walletHistoryService.RemoveAsync(context.HistoryOperationId);
                }

                if (tx.TransactionType == TransactionType.CashOut)
                {
                    var context = tx.ContextData.DeserializeJson<CashoutTransactionContext>();
                    if (context != null)
                        await _walletHistoryService.RemoveAsync(context.HistoryOperationId);
                }
            }
        }

        #endregion

        #region Commands mapping

        private static Action<IMappingOperationOptions> MapConfirmations(int confirmationsCount)
        {
            return opt => opt.Items["Confirmations"] = confirmationsCount;
        }

        private Action<IMappingOperationOptions> MapConfirmed()
        {
            return MapConfirmations(_transactionConfirmationCount);
        }

        private IUpdateTransactionCommand MapToUpdateCommand(
            RegisterInTxCommand cmd,
            TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Payment:
                    return Mapper.Map<UpdateTransactionCommand>(cmd, MapConfirmed());
                case TransactionType.Exchange:
                    return Mapper.Map<UpdateExchangeInTxCommand>(cmd, MapConfirmed());
                case TransactionType.Settlement:
                    return Mapper.Map<UpdateSettlementInTxCommand>(cmd, MapConfirmed());
                default:
                    throw new UnexpectedTransactionTypeException(transactionType);
            }
        }

        private IUpdateTransactionCommand MapToUpdateCommand(
            UpdateOutTxCommand cmd,
            IPaymentRequestTransaction tx)
        {
            switch (tx.TransactionType)
            {
                case TransactionType.Payment:
                    return Mapper.Map<UpdatePaymentOutTxCommand>(cmd);
                case TransactionType.Refund:
                    return Mapper.Map<UpdateRefundOutTxCommand>(cmd,
                        opt => opt.Items["WalletAddress"] = tx.WalletAddress);
                case TransactionType.Exchange:
                    return Mapper.Map<UpdateExchangeOutTxCommand>(cmd);
                case TransactionType.Settlement:
                    return Mapper.Map<UpdateSettlementOutTxCommand>(cmd);
                case TransactionType.CashOut:
                    return Mapper.Map<UpdateCashoutTxCommand>(cmd);
                default:
                    throw new UnexpectedTransactionTypeException(tx.TransactionType);
            }
        }

        private IUpdateTransactionCommand MapToUpdateCommand(
            CompleteOutTxCommand cmd,
            IPaymentRequestTransaction tx)
        {
            switch (tx.TransactionType)
            {
                case TransactionType.Payment:
                    return Mapper.Map<CompletePaymentOutTxCommand>(cmd, MapConfirmed());
                case TransactionType.Refund:
                    return Mapper.Map<CompleteRefundOutTxCommand>(cmd,
                        opt =>
                        {
                            opt.Items["Confirmations"] = _transactionConfirmationCount;
                            opt.Items["WalletAddress"] = tx.WalletAddress;
                        });
                case TransactionType.Exchange:
                    return Mapper.Map<CompleteExchangeOutTxCommand>(cmd, MapConfirmed());
                case TransactionType.Settlement:
                    return Mapper.Map<CompleteSettlementOutTxCommand>(cmd, MapConfirmed());
                case TransactionType.CashOut:
                    return Mapper.Map<CompleteCashoutTxCommand>(cmd, MapConfirmed());
                default:
                    throw new UnexpectedTransactionTypeException(tx.TransactionType);
            }
        }

        private IUpdateTransactionCommand MapToUpdateCommand(
            NotEnoughFundsOutTxCommand cmd,
            TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Payment:
                    return Mapper.Map<FailPaymentOutTxCommand>(cmd, MapConfirmed());
                case TransactionType.CashOut:
                    return Mapper.Map<FailCashoutTxCommand>(cmd, MapConfirmed());
                case TransactionType.Exchange:
                    return Mapper.Map<FailExchangeOutTxCommand>(cmd, MapConfirmed());
                default:
                    throw new UnexpectedTransactionTypeException(transactionType);
            }
        }

        private IUpdateTransactionCommand MapToUpdateCommand(
            FailOutTxCommand cmd,
            TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Payment:
                    return Mapper.Map<FailPaymentOutTxCommand>(cmd, MapConfirmed());
                case TransactionType.CashOut:
                    return Mapper.Map<FailCashoutTxCommand>(cmd, MapConfirmed());
                case TransactionType.Exchange:
                    return Mapper.Map<FailExchangeOutTxCommand>(cmd, MapConfirmed());
                default:
                    throw new UnexpectedTransactionTypeException(transactionType);
            }
        }

        private ICreateTransactionCommand MapToCreateCommand(RegisterInTxCommand cmd)
        {
            switch (cmd.WorkflowType)
            {
                case WorkflowType.LykkePay:
                    return Mapper.Map<CreateTransactionCommand>(cmd, MapConfirmed());
                case WorkflowType.Airlines:
                    return Mapper.Map<RegisterCashinTxCommand>(cmd, MapConfirmed());
                default:
                    throw new UnexpectedWorkflowTypeException(cmd.WorkflowType);
            }
        }

        #endregion
    }
}
