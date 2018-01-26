using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ISingleSourceTransferRequest : ITransferRequest
    {
        string SourceAddress { get; set; }
    }
}
