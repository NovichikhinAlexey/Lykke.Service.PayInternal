using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task<IReadOnlyList<IBlockchainTransaction>> GetAsync(string walletAddress);
        Task<IBlockchainTransaction> GetAsync(string walletAddress, string transactionId);
        Task InsertAsync(IBlockchainTransaction blockchainTransaction);
        Task UpdateAsync(IBlockchainTransaction blockchainTransaction);
    }
}
