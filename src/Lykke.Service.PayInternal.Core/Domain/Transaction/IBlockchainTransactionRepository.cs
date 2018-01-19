using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task SaveAsync(IBlockchainTransaction tx);
        Task InsertOrMergeAsync(IBlockchainTransaction tx);
        Task<IEnumerable<IBlockchainTransaction>> GetByWallet(string walletAddress);
    }
}
