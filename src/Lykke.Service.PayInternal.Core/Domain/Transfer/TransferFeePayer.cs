using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Describes, who does pay the fee for the transfer.
    /// </summary>
    public enum TransferFeePayerEnum
    {
        Merchant,
        Client
    }
}
