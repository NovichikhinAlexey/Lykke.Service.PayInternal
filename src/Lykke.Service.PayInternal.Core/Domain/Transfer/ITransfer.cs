using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransfer
    {
        string TransferId { get; set; }
        string TransactionHash { get; set; }
        TransferStatus TransferStatus { get; set; }
        TransferStatusError TransferStatusError { get; set; }
    }
}
