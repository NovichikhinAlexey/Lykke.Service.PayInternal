using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Transfer request with single address source
    /// </summary>
    public interface ISingleSourceTransferRequest : ITransferRequest
    {
        /// <summary>
        /// Source Address
        /// </summary>
        string SourceAddress { get; set; }
    }
}
