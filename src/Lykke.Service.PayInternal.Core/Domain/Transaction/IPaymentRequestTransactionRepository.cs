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
        /// <param name="transactionId"></param>
        /// <param name="blockchain"></param>
        /// <param name="walletAddress"></param>
        /// <returns></returns>
        Task<IPaymentRequestTransaction> GetByIdAsync(string transactionId, BlockchainType blockchain, string walletAddress);

        /// <summary>
        /// Gets multiple business transactions related to single blockchain transaction provided
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="blockchain"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetByTransactionIdAsync(string transactionId, BlockchainType blockchain);

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
        Task UpdateAsync(IPaymentRequestTransaction transaction);
    }
}
