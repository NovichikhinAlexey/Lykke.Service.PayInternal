using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly IBlockchainTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILog _log;

        public TransactionsService(
            IBlockchainTransactionRepository transactionRepository,
            IPaymentRequestRepository paymentRequestRepository,
            IOrderRepository ordersRepository,
            ILog log)
        {
            _transactionRepository = transactionRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _orderRepository = ordersRepository;
            _log = log;
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetAsync(string walletAddress)
        {
            return await _transactionRepository.GetAsync(walletAddress);
        }

        public async Task Create(ICreateTransaction request)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(request.WalletAddress);

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
                PaymentRequestId = paymentRequest.Id
            };

            await _transactionRepository.InsertAsync(transactionEntity);
        }

        public async Task Update(IUpdateTransaction request)
        {
            IBlockchainTransaction transaction =
                await _transactionRepository.GetAsync(request.WalletAddress, request.TransactionId);

            if (transaction == null)
                throw new Exception($"Transaction with id {request.TransactionId} doesn't exist");

            transaction.Amount = (decimal)request.Amount;
            transaction.BlockId = request.BlockId;
            transaction.Confirmations = request.Confirmations;

            await _transactionRepository.UpdateAsync(transaction);
        }
    }
}
