using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task<IReadOnlyList<IBlockchainTransaction>> GetAsync(string walletAddress);
        Task<IBlockchainTransaction> GetAsync(string walletAddress, string transactionId);
        Task<IEnumerable<IBlockchainTransaction>> GetByTransactionAsync(string transactionId);
        Task<IEnumerable<IBlockchainTransaction>> GetNotExpiredAsync(int minConfirmationsCount);
        Task<IBlockchainTransaction> AddAsync(IBlockchainTransaction blockchainTransaction);
        Task UpdateAsync(IBlockchainTransaction blockchainTransaction);
        Task<IReadOnlyList<IBlockchainTransaction>> GetByPaymentRequest(string paymentRequestId);
    }
}
