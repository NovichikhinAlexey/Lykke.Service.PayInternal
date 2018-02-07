using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Repository for transfer entity
    /// </summary>
    public interface ITransferRepository
    {
        /// <summary>
        /// Get all transfers
        /// </summary>
        /// <returns></returns>

        Task<IEnumerable<ITransferRequest>> GetAllAsync();
        /// <summary>
        /// Get a transfer entity with all transactions
        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <returns></returns>
        Task<ITransferRequest> GetAsync(string transferRequestId);
        /// <summary>
        /// et a transfer entity with a specify transaction

        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <param name="transactionHash">PRC Transaction Hash</param>
        /// <returns></returns>

        Task<ITransferRequest> GetAsync(string transferRequestId, string transactionHash);

        /// <summary>
        /// Save transfer info
        /// </summary>
        /// <param name="transferInfo">Transfer info</param>
        /// <returns></returns>

        Task<ITransferRequest> SaveAsync(ITransferRequest transferInfo);

    }
}
