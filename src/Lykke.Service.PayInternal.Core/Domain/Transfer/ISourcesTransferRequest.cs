using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ISourcesTransferRequest : ITransferRequest
    {
        IEnumerable<ISourceAmount> SourceAddresses { get; set; }
    }
}
