using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IPaymentRequestTransactionRepository
    {
        /// <summary>
        /// Gets all business transactions related to wallet address
        /// </summary>
        /// <param name="walletAddress"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByWalletAsync(string walletAddress);

        /// <summary>
        /// Gets business single transaction
        /// </summary>
        /// <param name="blockchain"></param>
        /// <param name="identityType"></param>
        /// <param name="identity"></param>
        /// <param name="walletAddress"></param>
        /// <returns></returns>
        Task<IPaymentRequestTransaction> GetByIdAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity, string walletAddress);

        /// <summary>
        /// Gets multiple business transactions related to single blockchain transaction provided
        /// </summary>
        /// <param name="blockchain"></param>
        /// <param name="identityType"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByBcnIdentityAsync(BlockchainType blockchain, TransactionIdentityType identityType, string identity);

        /// <summary>
        /// Gets business transactions filtered by DueDate  
        /// </summary>
        /// <param name="dueDateGreaterThan"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByDueDate(DateTime dueDateGreaterThan);

        /// <summary>
        /// Adds new business transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IPaymentRequestTransaction> AddAsync(IPaymentRequestTransaction transaction);

        /// <summary>
        /// Updates business transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<IPaymentRequestTransaction> UpdateAsync(IPaymentRequestTransaction transaction);
    }
}
