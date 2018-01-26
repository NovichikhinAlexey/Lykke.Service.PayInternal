using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Full transfer object. Contain full transaction fields
    /// </summary>
    public interface ITransferInfo: ITransfer
    {
        /// <summary>
        /// List of source amount pairs.
        /// </summary>
        IEnumerable<ISourceAmount> SourceAddresses { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        string DestinationAddress { get; set; }
        /// <summary>
        /// TotalAmount
        /// </summary>
        decimal Amount { get; set; }
        /// <summary>
        /// Currency
        /// </summary>
        string Currency { get; set; }
    }
}
