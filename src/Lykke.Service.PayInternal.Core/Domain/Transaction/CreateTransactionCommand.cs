using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public class CreateTransactionCommand : ICreateTransactionCommand
    {
        public string Hash { get; set; }
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
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public string ContextData { get; set; }
    }
}
