using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

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

        public async Task InsertAsync(IBlockchainTransaction blockchainTransaction)
        {
            var entity = new BlockchainTransactionEntity(
                GetPartitionKey(blockchainTransaction.WalletAddress),
                GetRowKey(blockchainTransaction.TransactionId));
            entity.Map(blockchainTransaction);

            await _storage.InsertAsync(entity);
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
