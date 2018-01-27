using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly IBlockchainTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITransactionUpdatesPublisher _updatesPublisher;
        private readonly ILog _log;

        public TransactionsService(
            IBlockchainTransactionRepository transactionRepository,
            IPaymentRequestRepository paymentRequestRepository,
            IOrderRepository ordersRepository,
            ITransactionUpdatesPublisher updatesPublisher,
            ILog log)
        {
            _transactionRepository = transactionRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _orderRepository = ordersRepository;
            _updatesPublisher = updatesPublisher;
            _log = log;
        }

        public async Task Create(ICreateTransaction request)
        {
            var order = await FindTransactionOrderByDate(request.WalletAddress, request.FirstSeen);

            var transactionEntity = new BlockchainTransactionEntity
            {
                WalletAddress = request.WalletAddress,
                TransactionId = request.TransactionId,
                Amount = request.Amount,
                Confirmations = request.Confirmations,
                BlockId = request.BlockId,
                FirstSeen = request.FirstSeen,
                OrderId = order.Id
            };

            await Task.WhenAll(
                _transactionRepository.SaveAsync(transactionEntity),
                _updatesPublisher.PublishAsync(transactionEntity.ToMessage()));
        }

        public async Task Update(IUpdateTransaction request)
        {
            var transaction = await _transactionRepository.Get(
                BlockchainTransactionEntity.ByWallet.GeneratePartitionKey(request.WalletAddress),
                BlockchainTransactionEntity.ByWallet.GenerateRowKey(request.TransactionId));

            if (transaction == null)
                throw new Exception($"Transaction with id {request.TransactionId} doesn't exist");

            transaction.Amount = request.Amount;
            transaction.BlockId = request.BlockId;
            transaction.Confirmations = request.Confirmations;

            await Task.WhenAll(
                _transactionRepository.MergeAsync(transaction),
                _updatesPublisher.PublishAsync(transaction.ToMessage()));
        }

        private async Task<IOrder> FindTransactionOrderByDate(string walletAddress, DateTime date)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);
            
            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);
            
            IReadOnlyList<IOrder> orders = await _orderRepository.GetAsync(paymentRequest.Id);

            IReadOnlyList<IOrder> dueDateOrders = orders.Where(x => date < x.DueDate).ToList();

            if (!dueDateOrders.Any())
                throw new Exception("There is no order with suitable DueDate");

            return dueDateOrders.Count > 1
                ? dueDateOrders.OrderBy(x => x.DueDate).First()
                : dueDateOrders.First();
        }
    }
}
