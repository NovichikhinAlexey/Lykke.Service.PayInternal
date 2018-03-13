using System;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CreateTransaction : ICreateTransaction
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public string[] SourceWalletAddresses { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public string Blockchain { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? DueDate { get; set; }
        public TransactionType Type { get; set; }
    }
}
