using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferInfo: ITransfer
    {
        IEnumerable<ISourceAmount> SourceAddresses { get; set; }
        string DestinationAddress { get; set; }
        double Amount { get; set; }
        string Currency { get; set; }
    }
}
