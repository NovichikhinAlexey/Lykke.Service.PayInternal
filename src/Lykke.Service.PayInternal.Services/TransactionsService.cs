using System.Collections;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class TransactionsService : ITransactionsService
    {
        private readonly IBlockchainTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        // ReSharper disable once NotAccessedField.Local
        private readonly IOrderRepository _orderRepository;
        private readonly int _transactionConfirmationCount;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public TransactionsService(
            IBlockchainTransactionRepository transactionRepository,
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

        public async Task<IEnumerable<IBlockchainTransaction>> GetAsync(string walletAddress)
        {
            return await _transactionRepository.GetAsync(walletAddress);
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetAllMonitoredAsync()
        {
            var activePayRequests = await _paymentRequestRepository.GetNotExpiredAsync();

            var payRequestIdList = activePayRequests
                .Select(pr => pr.Id)
                .ToList();

            var result = await _transactionRepository.GetNotExpiredAsync(
                payRequestIdList,
                _transactionConfirmationCount);

            return result;
        }

        public async Task Create(ICreateTransaction request, TransactionType transactionType)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(request.WalletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(request.WalletAddress);

            var transactionEntity = new BlockchainTransaction
            {
                WalletAddress = request.WalletAddress,
                TransactionId = request.TransactionId,
                Amount = (decimal)request.Amount,
                AssetId = request.AssetId,
                Confirmations = request.Confirmations,
                BlockId = request.BlockId,
                Blockchain = request.Blockchain,
                FirstSeen = request.FirstSeen,
                PaymentRequestId = paymentRequest.Id,
                TransactionType = transactionType
            };

            await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task Update(IUpdateTransaction request)
        {
            if (string.IsNullOrEmpty(request.WalletAddress))
            {
                IEnumerable<IBlockchainTransaction> transactions =
                    await _transactionRepository.GetByTransactionAsync(request.TransactionId);

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

            IBlockchainTransaction transaction =
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
