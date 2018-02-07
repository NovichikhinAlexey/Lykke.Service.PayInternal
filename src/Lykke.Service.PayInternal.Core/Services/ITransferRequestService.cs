using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    /// <summary>
    /// Transfer Request Service. Make Transfers. Update States. Get Transfer Status.
    /// </summary>
    public interface ITransferRequestService
    {
        /// <summary>
        /// Create transfer using transfer request.
        /// </summary>
        /// <param name="transferRequest">transfer request</param>
        /// <returns></returns>

        Task<ITransferRequest> CreateTransferAsync(ITransferRequest transferRequest);

        /// <summary>
        /// Update transfer status. Other fields will be ignored
        /// </summary>
        /// <param name="transfer">Transfer</param>
        /// <returns></returns>

        Task<ITransferRequest> UpdateTransferStatusAsync(ITransferRequest transfer);

        /// <summary>
        /// Update / insert transfer entity
        /// </summary>
        /// <param name="transfer">transfer entity</param>
        /// <returns></returns>

        Task<ITransferRequest> UpdateTransferAsync(ITransferRequest transfer);

        /// <summary>
        /// Get transfer entity
        /// </summary>
        /// <param name="transfer">shord transfer structure</param>
        /// <returns></returns>

        Task<ITransferRequest> GetTransferInfoAsync(ITransferRequest transfer);

    }
}
