using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
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

        public async Task AddAsync(IBlockchainTransaction blockchainTransaction)
        {
            var entity = new BlockchainTransactionEntity(
                GetPartitionKey(blockchainTransaction.WalletAddress),
                GetRowKey(blockchainTransaction.TransactionId));
            entity.Map(blockchainTransaction);

            await _storage.InsertOrMergeAsync(entity);
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetNotExpiredAsync(IReadOnlyList<string> paymentRequestIdList, int minConfirmationsCount)
        {
            var result = new List<IBlockchainTransaction>();

            foreach (var payReqId in paymentRequestIdList)
            {
                var filter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PaymentRequestId", QueryComparisons.Equal, payReqId),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForInt("Confirmations", QueryComparisons.LessThan, minConfirmationsCount));

                var query = new TableQuery<BlockchainTransactionEntity>().Where(filter);

                var item = await _storage.WhereAsync(query);
                if (item != null)
                    result.AddRange(item);
            }

            return result;
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
