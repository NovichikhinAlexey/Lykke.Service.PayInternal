using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    public class PaymentRequestTransactionRepository : IPaymentRequestTransactionRepository
    {
        private readonly INoSQLTableStorage<PaymentRequestTransactionEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByTransactionIdStorage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByDueDateStorage;

        public PaymentRequestTransactionRepository(
            INoSQLTableStorage<PaymentRequestTransactionEntity> storage,
            INoSQLTableStorage<AzureIndex> indexByTransactionIdStorage,
            INoSQLTableStorage<AzureIndex> indexByDueDateStorage)
        {
            _storage = storage;
            _indexByTransactionIdStorage = indexByTransactionIdStorage;
            _indexByDueDateStorage = indexByDueDateStorage;
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByPaymentRequestAsync(string paymentRequestId)
        {
            IList<PaymentRequestTransactionEntity> entities =
                await _storage.GetDataAsync(x => x.PaymentRequestId == paymentRequestId);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
        }

        public async Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction)
        {
            PaymentRequestTransactionEntity
                entity = PaymentRequestTransactionEntity.ByWalletAddress.Create(transaction);

            await _storage.InsertOrMergeAsync(entity);

            AzureIndex indexByTransactionId = PaymentRequestTransactionEntity.IndexByTransactionId.Create(entity);

            await _indexByTransactionIdStorage.InsertOrMergeAsync(indexByTransactionId);

            AzureIndex indexByDueDate = PaymentRequestTransactionEntity.IndexByDueDate.Create(entity);

            await _indexByDueDateStorage.InsertOrMergeAsync(indexByDueDate);

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress)
        {
            string partitionKey = PaymentRequestTransactionEntity.ByWalletAddress.GeneratePartitionKey(walletAddress);

            IEnumerable<PaymentRequestTransactionEntity> entities = await _storage.GetDataAsync(partitionKey);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
        }

        public async Task<IPaymentRequestTransaction> GetByIdAsync(string transactionId,
            BlockchainType blockchain)
        {
            AzureIndex index = await _indexByTransactionIdStorage.GetDataAsync(
                PaymentRequestTransactionEntity.IndexByTransactionId.GeneratePartitionKey(transactionId),
                PaymentRequestTransactionEntity.IndexByTransactionId.GenerateRowKey(blockchain));

            if (index == null) return null;

            PaymentRequestTransactionEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByDueDate(DateTime dueDateGreaterThan)
        {
            string gtDate = PaymentRequestTransactionEntity.IndexByDueDate.GeneratePartitionKey(dueDateGreaterThan);

            // fake date to fit partitionKey format and not to get data from other partitions 
            string ltDate =
                PaymentRequestTransactionEntity.IndexByDueDate.GeneratePartitionKey(dueDateGreaterThan.AddYears(100));

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, gtDate),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, ltDate));

            var query = new TableQuery<AzureIndex>().Where(filter);

            IEnumerable<AzureIndex> indecies = await _indexByDueDateStorage.WhereAsync(query);

            IEnumerable<PaymentRequestTransactionEntity> entities = await _storage.GetDataAsync(indecies);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
        }

        public async Task UpdateAsync(IPaymentRequestTransaction transaction)
        {
            await _storage.MergeAsync(
                PaymentRequestTransactionEntity.ByWalletAddress.GeneratePartitionKey(transaction.WalletAddress),
                PaymentRequestTransactionEntity.ByWalletAddress.GenerateRowKey(transaction.TransactionId),
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
    }
}
