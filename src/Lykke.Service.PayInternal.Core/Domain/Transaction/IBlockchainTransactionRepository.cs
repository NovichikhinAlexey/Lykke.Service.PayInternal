using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task SaveAsync(IBlockchainTransaction tx);
        Task<IBlockchainTransaction> Get(string walletAddress, string txId);
        Task<IBlockchainTransaction> InsertOrMergeAsync(IBlockchainTransaction tx);
        Task<IBlockchainTransaction> MergeAsync(IBlockchainTransaction tx);
        Task<IEnumerable<IBlockchainTransaction>> GetByWallet(string walletAddress);
    }
}
