using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Order
{
    public class CalculatedAmountResponse
    {
        public decimal PaymentAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
