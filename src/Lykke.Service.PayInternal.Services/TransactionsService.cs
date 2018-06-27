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
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class TransactionsService : ITransactionsService
    {
        private readonly IPaymentRequestTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IOrderService _orderService;
        private readonly int _transactionConfirmationCount;

        public TransactionsService(
            [NotNull] IPaymentRequestTransactionRepository transactionRepository,
            [NotNull] IPaymentRequestRepository paymentRequestRepository,
            int transactionConfirmationCount,
            [NotNull] IOrderService orderService)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _paymentRequestRepository = paymentRequestRepository ?? throw new ArgumentNullException(nameof(paymentRequestRepository));
            _transactionConfirmationCount = transactionConfirmationCount;
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress)
        {
            return _transactionRepository.GetByWalletAsync(walletAddress);
        }

        public Task<IPaymentRequestTransaction> GetByIdAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity, string walletAddress)
        {
            return _transactionRepository.GetByIdAsync(blockchain, identityType, identity, walletAddress);
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetByBcnIdentityAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity)
        {
            return await _transactionRepository.GetByBcnIdentityAsync(blockchain, identityType, identity);
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
            IPaymentRequest paymentRequest = null;

            if (!string.IsNullOrEmpty(request.WalletAddress))
            {
                paymentRequest = await _paymentRequestRepository.FindAsync(request.WalletAddress);

                if (paymentRequest == null)
                    throw new PaymentRequestNotFoundException(request.WalletAddress);

                IPaymentRequestTransaction existing =
                    await _transactionRepository.GetByIdAsync(request.Blockchain, request.IdentityType, request.Identity, request.WalletAddress);

                if (existing != null)
                {
                    await UpdateAsync(Mapper.Map<UpdateTransactionCommand>(request));

                    return await _transactionRepository.GetByIdAsync(request.Blockchain, request.IdentityType,
                        request.Identity, request.WalletAddress);
                }
            }

            var transactionEntity =
                Mapper.Map<PaymentRequestTransaction>(request, opts => opts.Items["PaymentRequest"] = paymentRequest);

            return await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task<IPaymentRequestTransaction> CreateLykkeTransactionAsync(ICreateLykkeTransactionCommand request)
        {
            IOrder order = await _orderService.GetByLykkeOperationAsync(request.OperationId);

            if (order == null)
                throw new LykkeOperationOrderNotFoundException(request.OperationId);

            IPaymentRequest paymentRequest =
                await _paymentRequestRepository.GetAsync(order.MerchantId, order.PaymentRequestId);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(order.MerchantId, order.PaymentRequestId);

            var tx = new PaymentRequestTransaction
            {
                Blockchain = BlockchainType.Lykke,
                Amount = request.Amount,
                AssetId = request.AssetId,
                IdentityType = request.IdentityType,
                Identity = request.Identity,
                Confirmations = request.Confirmations,
                CreatedOn = DateTime.UtcNow,
                SourceWalletAddresses = request.SourceWalletAddresses,
                TransactionType = request.Type,
                PaymentRequestId = paymentRequest.Id,
                WalletAddress = paymentRequest.WalletAddress,
                DueDate = paymentRequest.DueDate,
            };

            return await _transactionRepository.AddAsync(tx);
        }

        public async Task UpdateAsync(IUpdateTransactionCommand request)
        {
            var businessTransactions = new List<IPaymentRequestTransaction>();

            if (!string.IsNullOrEmpty(request.WalletAddress))
            {
                IPaymentRequestTransaction tx = await _transactionRepository.GetByIdAsync(request.Blockchain,
                    request.IdentityType, request.Identity, request.WalletAddress);

                if (tx == null)
                    throw new TransactionNotFoundException(request.Blockchain, request.IdentityType, request.Identity, request.WalletAddress);

                businessTransactions.Add(tx);
            }
            else
            {
                IReadOnlyList<IPaymentRequestTransaction> txs =
                    await _transactionRepository.GetByBcnIdentityAsync(request.Blockchain, request.IdentityType, request.Identity);

                businessTransactions.AddRange(txs);
            }

            foreach (IPaymentRequestTransaction tx in businessTransactions)
            {
                tx.BlockId = request.BlockId;
                tx.Confirmations = request.Confirmations;
                tx.FirstSeen = request.FirstSeen;

                if (request.IsPayment())
                {
                    tx.Amount = request.Amount;
                }

                if (tx.IdentityType != TransactionIdentityType.Hash)
                {
                    tx.TransactionId = request.Hash;
                }

                await _transactionRepository.UpdateAsync(tx);
            }
        }
    }
}
