using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    public class PaymentRequestTransactionRepository : IPaymentRequestTransactionRepository
    {
        private readonly INoSQLTableStorage<PaymentRequestTransactionEntity> _storage;

        public PaymentRequestTransactionRepository(INoSQLTableStorage<PaymentRequestTransactionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetAsync(string walletAddress)
        {
            IEnumerable<PaymentRequestTransactionEntity> entities =
                await _storage.GetDataAsync(GetPartitionKey(walletAddress));

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
        }
        
        public async Task<IPaymentRequestTransaction> GetAsync(string walletAddress, string transactionId)
        {
            PaymentRequestTransactionEntity entity =
                await _storage.GetDataAsync(GetPartitionKey(walletAddress), GetRowKey(transactionId));

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetByTransactionAsync(string transactionId)
        {
            IEnumerable<PaymentRequestTransactionEntity> entities =
                await _storage.GetDataAsync(t => t.Id == transactionId);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities);
        }

        public async Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction)
        {
            var entity = new PaymentRequestTransactionEntity(
                GetPartitionKey(transaction.WalletAddress),
                GetRowKey(transaction.TransactionId));

            Mapper.Map(transaction, entity);

            await _storage.InsertOrMergeAsync(entity);

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }

        public async Task<IEnumerable<IPaymentRequestTransaction>> GetNotExpiredAsync(int minConfirmationsCount)
        {
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDate("DueDate", QueryComparisons.GreaterThan, DateTime.UtcNow),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForInt("Confirmations", QueryComparisons.LessThan, minConfirmationsCount));

            var query = new TableQuery<PaymentRequestTransactionEntity>().Where(filter);

            IEnumerable<PaymentRequestTransactionEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities);
        }

        public async Task UpdateAsync(IPaymentRequestTransaction transaction)
        {
            await _storage.MergeAsync(
                GetPartitionKey(transaction.WalletAddress),
                GetRowKey(transaction.TransactionId),
                entity =>
                {
                    entity.Amount = transaction.Amount;
                    entity.BlockId = transaction.BlockId;
                    entity.Confirmations = transaction.Confirmations;
                    entity.FirstSeen = transaction.FirstSeen;
                    entity.PaymentRequestId = transaction.PaymentRequestId;

                    return entity;
                });
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByPaymentRequest(string paymentRequestId)
        {
            // todo: optimize transactions repository

            string filter = TableQuery.GenerateFilterCondition("PaymentRequestId", QueryComparisons.Equal, paymentRequestId);

            var query = new TableQuery<PaymentRequestTransactionEntity>().Where(filter);

            IEnumerable<PaymentRequestTransactionEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
        }

        private static string GetPartitionKey(string walletAddress)
            => walletAddress;

        private static string GetRowKey(string transactionId)
            => transactionId;
    }
}
