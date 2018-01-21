using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    public class BlockchainTransactionRepository : IBlockchainTransactionRepository
    {
        private readonly INoSQLTableStorage<BlockchainTransactionEntity> _tableStorage;

        public BlockchainTransactionRepository(INoSQLTableStorage<BlockchainTransactionEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task SaveAsync(IBlockchainTransaction tx)
        {
            var newItem = BlockchainTransactionEntity.ByWallet.Create(tx);

            await _tableStorage.InsertAsync(newItem);
        }

        public async Task<IBlockchainTransaction> Get(string walletAddress, string txId)
        {
            return await _tableStorage.GetDataAsync(
                BlockchainTransactionEntity.ByWallet.GeneratePartitionKey(walletAddress),
                BlockchainTransactionEntity.ByWallet.GenerateRowKey(txId));
        }

        public async Task<IBlockchainTransaction> InsertOrMergeAsync(IBlockchainTransaction tx)
        {
            var item = BlockchainTransactionEntity.ByWallet.Create(tx);

            await _tableStorage.InsertOrMergeAsync(item);

            return item;
        }

        public async Task<IBlockchainTransaction> MergeAsync(IBlockchainTransaction tx)
        {
            return await _tableStorage.MergeAsync(
                BlockchainTransactionEntity.ByWallet.GeneratePartitionKey(tx.WalletAddress),
                BlockchainTransactionEntity.ByWallet.GenerateRowKey(tx.Id),
                entity =>
                {
                    entity.Amount = tx.Amount;
                    entity.BlockId = tx.BlockId;
                    entity.Confirmations = tx.Confirmations;
                    entity.FirstSeen = tx.FirstSeen;
                    entity.OrderId = tx.OrderId;

                    return entity;
                });
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetByWallet(string walletAddress)
        {
            return await _tableStorage.GetDataAsync(
                BlockchainTransactionEntity.ByWallet.GeneratePartitionKey(walletAddress));
        }
    }
}
