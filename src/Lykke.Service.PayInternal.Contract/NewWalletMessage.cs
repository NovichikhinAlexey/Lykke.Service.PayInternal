using System;

namespace Lykke.Service.PayInternal.Contract
{
    public class NewWalletMessage
    {
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
    }
}
