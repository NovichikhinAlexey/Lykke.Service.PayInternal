using System;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<IPaymentRequestTransaction> GetByIdAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity)
        {
            return await _transactionRepository.GetByIdAsync(blockchain, identityType, identity);
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

            var transactionEntity =
                Mapper.Map<PaymentRequestTransaction>(request, opts => opts.Items["PaymentRequest"] = paymentRequest);

            return await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task UpdateAsync(IUpdateTransactionCommand request)
        {
            IPaymentRequestTransaction transaction =
                await _transactionRepository.GetByIdAsync(request.Blockchain, request.IdentityType, request.Identity);

            if (transaction == null)
                throw new TransactionNotFoundException(request.Blockchain, request.IdentityType, request.Identity);

            if (request.IdentityType != TransactionIdentityType.Hash)
            {
                transaction.TransactionId = request.Hash;
            }

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
