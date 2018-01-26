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
        Task<IEnumerable<ITransferInfo>> GetAllAsync();
        /// <summary>
        /// Get all transfers entity if transactions are more than one for the transfer
        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <returns></returns>
        Task<IEnumerable<ITransferInfo>> GetAsync(string transferRequestId);
        /// <summary>
        /// Get transfer for exact transaction hash
        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <param name="transactionHash">PRC Transaction Hash</param>
        /// <returns></returns>
        Task<ITransferInfo> GetAsync(string transferRequestId, string transactionHash);
        /// <summary>
        /// Save transfer info
        /// </summary>
        /// <param name="transferInfo">Transfer info</param>
        /// <returns></returns>
        Task<ITransferInfo> SaveAsync(ITransferInfo transferInfo);
    }
}
