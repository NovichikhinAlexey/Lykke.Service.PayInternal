using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models
{
    public class WalletStateResponse
    {
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
        public IEnumerable<PayTransactionStateResponse> Transactions { get; set; }
    }

    public class PayTransactionStateResponse
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockId { get; set; }
        public string Blockchain { get; set; }
        public int Confirmations { get; set; }
        public string WalletAddress { get; set; }
    }
}
