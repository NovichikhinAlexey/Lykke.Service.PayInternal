using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class TransactionsService : ITransactionsService
    {
        private readonly IPaymentRequestTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        // ReSharper disable once NotAccessedField.Local
        private readonly IOrderRepository _orderRepository;
        private readonly int _transactionConfirmationCount;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public TransactionsService(
            IPaymentRequestTransactionRepository transactionRepository,
            IPaymentRequestRepository paymentRequestRepository,
            IOrderRepository ordersRepository,
            int transactionConfirmationCount,
            ILog log)
        {
            _transactionRepository = transactionRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _orderRepository = ordersRepository;
            _transactionConfirmationCount = transactionConfirmationCount;
            _log = log;
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetAsync(string walletAddress)
        {
            return await _transactionRepository.GetAsync(walletAddress);
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetConfirmedAsync(string walletAddress)
        {
            var transactions = await GetAsync(walletAddress);
            return transactions?
                .Where(t => t.Confirmations >= _transactionConfirmationCount);
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetAllMonitoredAsync()
        {
            var result = await _transactionRepository.GetNotExpiredAsync(_transactionConfirmationCount);

            return result;
        }

        public async Task<IPaymentRequestTransaction> CreateTransaction(ICreateTransaction request)
        {
            var paymentRequest = await _paymentRequestRepository.FindAsync(request.WalletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(request.WalletAddress);

            var transactionEntity = new PaymentRequestTransaction
            {
                WalletAddress = request.WalletAddress,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                AssetId = request.AssetId,
                Confirmations = request.Confirmations,
                BlockId = request.BlockId,
                Blockchain = request.Blockchain.ToString(),
                FirstSeen = request.FirstSeen,
                PaymentRequestId = paymentRequest.Id,
                SourceWalletAddresses = request.SourceWalletAddresses,
                TransactionType = request.Type,
                DueDate = request.DueDate ?? paymentRequest.DueDate,
                TransferId = request.TransferId
            };

            return await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task Update(IUpdateTransaction request)
        {
            if (string.IsNullOrEmpty(request.WalletAddress))
            {
                IEnumerable<IPaymentRequestTransaction> transactions =
                    (await _transactionRepository.GetByTransactionAsync(request.TransactionId)).ToList();

                if (!transactions.Any())
                    throw new TransactionNotFoundException(request.TransactionId);

                foreach (var bcnTransaction in transactions)
                {
                    bcnTransaction.BlockId = request.BlockId;
                    bcnTransaction.FirstSeen = request.FirstSeen;
                    bcnTransaction.Confirmations = request.Confirmations;

                    await _transactionRepository.UpdateAsync(bcnTransaction);
                }

                return;
            }

            // payment transaction update
            IPaymentRequestTransaction transaction =
                await _transactionRepository.GetAsync(request.WalletAddress, request.TransactionId);

            if (transaction == null)
                throw new TransactionNotFoundException(request.TransactionId);

            transaction.Amount = (decimal)request.Amount;
            transaction.BlockId = request.BlockId;
            transaction.Confirmations = request.Confirmations;

            await _transactionRepository.UpdateAsync(transaction);
        }
    }
}
