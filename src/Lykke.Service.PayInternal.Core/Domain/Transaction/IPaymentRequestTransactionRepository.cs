using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IPaymentRequestTransactionRepository
    {
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetAsync(string walletAddress);
        Task<IPaymentRequestTransaction> GetAsync(string walletAddress, string transactionId);
        Task<IEnumerable<IPaymentRequestTransaction>> GetByTransactionAsync(string transactionId);
        Task<IEnumerable<IPaymentRequestTransaction>> GetNotExpiredAsync(int minConfirmationsCount);
        Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction);
        Task UpdateAsync(IPaymentRequestTransaction transaction);
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByPaymentRequest(string paymentRequestId);
    }
}
