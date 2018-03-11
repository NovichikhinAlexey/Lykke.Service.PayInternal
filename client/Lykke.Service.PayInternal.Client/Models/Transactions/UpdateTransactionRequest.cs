using System;

namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class UpdateTransactionRequest
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public double Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime? FirstSeen { get; set; }
    }
}
