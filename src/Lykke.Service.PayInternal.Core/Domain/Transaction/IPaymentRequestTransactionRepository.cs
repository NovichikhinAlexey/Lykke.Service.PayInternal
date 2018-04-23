using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IPaymentRequestTransactionRepository
    {
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress);

        Task<IPaymentRequestTransaction> GetByIdAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity);

        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByDueDate(DateTime dueDateGreaterThan);

        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByPaymentRequestAsync(string paymentRequestId);

        Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction);

        Task UpdateAsync(IPaymentRequestTransaction transaction);
    }
}
