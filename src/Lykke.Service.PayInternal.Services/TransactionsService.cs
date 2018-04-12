using System;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class TransactionsService : ITransactionsService
    {
        private readonly IPaymentRequestTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly int _transactionConfirmationCount;

        public TransactionsService(
            IPaymentRequestTransactionRepository transactionRepository,
            IPaymentRequestRepository paymentRequestRepository,
            int transactionConfirmationCount)
        {
            _transactionRepository = transactionRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _transactionConfirmationCount = transactionConfirmationCount;
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress)
        {
            return await _transactionRepository.GetByWalletAsync(walletAddress);
        }

        public async Task<IPaymentRequestTransaction> GetByIdAsync(string transactionId, BlockchainType blockchain)
        {
            return await _transactionRepository.GetByIdAsync(transactionId, blockchain);
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetConfirmedAsync(string walletAddress)
        {
            var transactions = await GetByWalletAsync(walletAddress);

            return transactions.Where(t => t.Confirmations >= _transactionConfirmationCount).ToList();
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetNotExpiredAsync()
        {
            var transactions = await _transactionRepository.GetByDueDate(DateTime.UtcNow);

            return transactions.Where(x => x.Confirmations < _transactionConfirmationCount).ToList();
        }

        public async Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand request)
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
                Blockchain = request.Blockchain,
                FirstSeen = request.FirstSeen,
                PaymentRequestId = paymentRequest.Id,
                SourceWalletAddresses = request.SourceWalletAddresses,
                TransactionType = request.Type,
                DueDate = request.DueDate ?? paymentRequest.DueDate,
                TransferId = request.TransferId
            };

            return await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task UpdateAsync(IUpdateTransactionCommand request)
        {
            IPaymentRequestTransaction transaction =
                await _transactionRepository.GetByIdAsync(request.TransactionId, request.Blockchain);

            if (transaction == null)
                throw new TransactionNotFoundException(request.TransactionId, request.Blockchain);

            transaction.BlockId = request.BlockId;
            transaction.Confirmations = request.Confirmations;
            transaction.FirstSeen = request.FirstSeen;

            if (request.IsPayment())
            {
                transaction.Amount = (decimal) request.Amount;
            }

            await _transactionRepository.UpdateAsync(transaction);
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetTransactionsByPaymentRequestAsync(string paymentRequestId)
        {
            IReadOnlyList<IPaymentRequestTransaction> transactions =
                (await _transactionRepository.GetByPaymentRequestAsync(paymentRequestId)).Where(x => x.IsPayment()).ToList();

            if (!transactions.Any())
                return null;

            IEnumerable<string> transferIds = transactions.Unique(x => x.TransferId).ToList();

            if (transferIds.MoreThanOne())
                throw new MultiTransactionRefundNotSupportedException();

            return transactions;
        }
    }
}
