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

        public async Task InsertOrMergeAsync(IBlockchainTransaction tx)
        {
            var item = BlockchainTransactionEntity.ByWallet.Create(tx);

            await _tableStorage.InsertOrMergeAsync(item);
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetByWallet(string walletAddress)
        {
            return await _tableStorage.GetDataAsync(
                BlockchainTransactionEntity.ByWallet.GeneratePartitionKey(walletAddress));
        }
    }
}
