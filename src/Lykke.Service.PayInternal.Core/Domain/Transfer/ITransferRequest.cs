using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferRequest
    {
        string MerchantId { get; set; }
        string DestinationAddress { get; set; }
        double Amount { get; set; }
        string Currency { get; set; }
    }
}
