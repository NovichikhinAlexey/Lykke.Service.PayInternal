using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models
{
    public class WalletStateResponse
    {
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
        public IEnumerable<TransactionStateResponse> Transactions { get; set; }
    }

    public class TransactionStateResponse
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string BlockId { get; set; }
        public int Confirmations { get; set; }
        public string WalletAddress { get; set; }
    }
}
