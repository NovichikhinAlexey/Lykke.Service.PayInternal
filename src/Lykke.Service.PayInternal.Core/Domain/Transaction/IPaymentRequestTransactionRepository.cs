using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IPaymentRequestTransactionRepository
    {
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress);

        Task<IPaymentRequestTransaction> GetByIdAsync(string transactionId, BlockchainType blockchain);

        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByDueDate(DateTime dueDateGreaterThan);

        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByPaymentRequestAsync(string paymentRequestId);

        Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction);

        Task UpdateAsync(IPaymentRequestTransaction transaction);
    }
}
