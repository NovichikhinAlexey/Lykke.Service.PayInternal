using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models
{
    public class WalletStateResponse
    {
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
        public IEnumerable<string> Transactions { get; set; }
    }
}
