using System;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CreateTransactionCommand : ICreateTransactionCommand
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public string[] SourceWalletAddresses { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public BlockchainType Blockchain { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? DueDate { get; set; }
        public TransactionType Type { get; set; }
        public string TransferId { get; set; }
    }
}
