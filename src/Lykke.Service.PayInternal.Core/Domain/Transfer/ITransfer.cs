using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Base transfer object
    /// </summary>
    public interface ITransfer
    {
        /// <summary>
        /// Id of transfer
        /// </summary>
        string TransferId { get; set; }
        /// <summary>
        /// Rpc transaction hash
        /// </summary>
        string TransactionHash { get; set; }
        /// <summary>
        /// Transfer Status
        /// </summary>
        TransferStatus TransferStatus { get; set; }
        /// <summary>
        /// Transfer Error Description is transaction fail
        /// </summary>
        TransferStatusError TransferStatusError { get; set; }
    }
}
