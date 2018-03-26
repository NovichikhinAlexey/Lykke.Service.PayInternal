using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransaction
    {
        string TransactionId { get; set; }
        string WalletAddress { get; set; }
        string[] SourceWalletAddresses { get; set; }
        decimal Amount { get; set; }
        string AssetId { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        string Blockchain { get; set; }
        DateTime? FirstSeen { get; set; }
        DateTime? DueDate { get; set; }
        TransactionType Type { get; set; }
        string TransferId { get; set; }
    }
}
