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
        private readonly INoSQLTableStorage<AzureIndex> _indexByIdentityStorage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByDueDateStorage;

        public PaymentRequestTransactionRepository(
            INoSQLTableStorage<PaymentRequestTransactionEntity> storage,
            INoSQLTableStorage<AzureIndex> indexByIdentityStorage,
            INoSQLTableStorage<AzureIndex> indexByDueDateStorage)
        {
            _storage = storage;
            _indexByIdentityStorage = indexByIdentityStorage;
            _indexByDueDateStorage = indexByDueDateStorage;
        }

        public async Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction)
        {
            PaymentRequestTransactionEntity
                entity = PaymentRequestTransactionEntity.ByWalletAddress.Create(transaction);

            await _storage.InsertOrMergeAsync(entity);

            AzureIndex indexByIdentity = PaymentRequestTransactionEntity.IndexByIdentity.Create(entity);

            await _indexByIdentityStorage.InsertOrMergeAsync(indexByIdentity);

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

        public async Task<IPaymentRequestTransaction> GetByIdAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity, string walletAddress)
        {
            AzureIndex index = await _indexByIdentityStorage.GetDataAsync(
                PaymentRequestTransactionEntity.IndexByIdentity.GeneratePartitionKey(blockchain, identityType, identity),
                PaymentRequestTransactionEntity.IndexByIdentity.GenerateRowKey(walletAddress));

            if (index == null) return null;

            PaymentRequestTransactionEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }

        public async Task<IReadOnlyList<IPaymentRequestTransaction>> GetByBcnIdentityAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity)
        {
            IEnumerable<AzureIndex> indecies = await _indexByIdentityStorage.GetDataAsync(
                PaymentRequestTransactionEntity.IndexByIdentity.GeneratePartitionKey(blockchain, identityType, identity));

            IEnumerable<PaymentRequestTransactionEntity> entities = await _storage.GetDataAsync(indecies);

            return Mapper.Map<IEnumerable<PaymentRequestTransaction>>(entities).ToList();
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

        public async Task<IPaymentRequestTransaction> UpdateAsync(IPaymentRequestTransaction transaction)
        {
            PaymentRequestTransactionEntity entity = await _storage.MergeAsync(
                PaymentRequestTransactionEntity.ByWalletAddress.GeneratePartitionKey(transaction.WalletAddress),
                PaymentRequestTransactionEntity.ByWalletAddress.GenerateRowKey(transaction.TransactionId),
                e =>
                {
                    e.TransactionId = transaction.TransactionId;
                    e.Amount = transaction.Amount;
                    e.BlockId = transaction.BlockId;
                    e.Confirmations = transaction.Confirmations;
                    e.FirstSeen = transaction.FirstSeen;
                    e.PaymentRequestId = transaction.PaymentRequestId;

                    return e;
                });

            return Mapper.Map<PaymentRequestTransaction>(entity);
        }
    }
}
