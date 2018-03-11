using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    public class BlockchainTransactionRepository : IBlockchainTransactionRepository
    {
        private readonly INoSQLTableStorage<BlockchainTransactionEntity> _storage;

        public BlockchainTransactionRepository(INoSQLTableStorage<BlockchainTransactionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<IBlockchainTransaction>> GetAsync(string walletAddress)
        {
            IEnumerable<BlockchainTransactionEntity> entities =
                await _storage.GetDataAsync(GetPartitionKey(walletAddress));

            return entities.ToList();
        }
        
        public async Task<IBlockchainTransaction> GetAsync(string walletAddress, string transactionId)
        {
            return await _storage.GetDataAsync(GetPartitionKey(walletAddress), GetRowKey(transactionId));
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetByTransactionAsync(string transactionId)
        {
            return await _storage.GetDataAsync(t => t.Id == transactionId);
        }

        public async Task<IBlockchainTransaction> AddAsync(IBlockchainTransaction blockchainTransaction)
        {
            var entity = new BlockchainTransactionEntity(
                GetPartitionKey(blockchainTransaction.WalletAddress),
                GetRowKey(blockchainTransaction.TransactionId));
            entity.Map(blockchainTransaction);

            await _storage.InsertOrMergeAsync(entity);

            return entity;
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetNotExpiredAsync(int minConfirmationsCount)
        {
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDate("DueDate", QueryComparisons.GreaterThan, DateTime.UtcNow),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForInt("Confirmations", QueryComparisons.LessThan, minConfirmationsCount));

            var query = new TableQuery<BlockchainTransactionEntity>().Where(filter);

            return await _storage.WhereAsync(query);
        }

        public async Task UpdateAsync(IBlockchainTransaction blockchainTransaction)
        {
            await _storage.MergeAsync(
                GetPartitionKey(blockchainTransaction.WalletAddress),
                GetRowKey(blockchainTransaction.TransactionId),
                entity =>
                {
                    entity.Amount = blockchainTransaction.Amount;
                    entity.BlockId = blockchainTransaction.BlockId;
                    entity.Confirmations = blockchainTransaction.Confirmations;
                    entity.FirstSeen = blockchainTransaction.FirstSeen;
                    entity.PaymentRequestId = blockchainTransaction.PaymentRequestId;

                    return entity;
                });
        }

        private static string GetPartitionKey(string walletAddress)
            => walletAddress;

        private static string GetRowKey(string transactionId)
            => transactionId;
    }
}
