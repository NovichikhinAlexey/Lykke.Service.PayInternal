using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Orders
{
    public interface ICalculatedAmountInfo
    {
        decimal PaymentAmount { get; set; }
        decimal ExchangeRate { get; set; }
    }
}
