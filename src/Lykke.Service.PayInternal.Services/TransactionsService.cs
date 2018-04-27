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

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByTransactionIdAsync(string transactionId,
            BlockchainType blockchain)
        {
            return await _transactionRepository.GetByTransactionIdAsync(transactionId, blockchain);
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
            var businessTransactions = new List<IPaymentRequestTransaction>();

            if (!string.IsNullOrEmpty(request.WalletAddress))
            {
                IPaymentRequestTransaction tx = await _transactionRepository.GetByIdAsync(request.TransactionId,
                    request.Blockchain, request.WalletAddress);

                businessTransactions.Add(tx);
            }
            else
            {
                IReadOnlyList<IPaymentRequestTransaction> txs =
                    await _transactionRepository.GetByTransactionIdAsync(request.TransactionId, request.Blockchain);

                businessTransactions.AddRange(txs);
            }

            foreach (IPaymentRequestTransaction tx in businessTransactions)
            {
                tx.BlockId = request.BlockId;
                tx.Confirmations = request.Confirmations;
                tx.FirstSeen = request.FirstSeen;

                if (request.IsPayment())
                {
                    tx.Amount = (decimal) request.Amount;
                }

                await _transactionRepository.UpdateAsync(tx);
            }
        }
    }
}
