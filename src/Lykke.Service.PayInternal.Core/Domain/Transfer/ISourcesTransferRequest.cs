using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// TransferRequest with a list of source amount pairs
    /// </summary>
    public interface ISourcesTransferRequest : ITransferRequest
    {
        /// <summary>
        /// List of source address pairs
        /// </summary>
        IEnumerable<ISourceAmount> SourceAddresses { get; set; }
    }
}
