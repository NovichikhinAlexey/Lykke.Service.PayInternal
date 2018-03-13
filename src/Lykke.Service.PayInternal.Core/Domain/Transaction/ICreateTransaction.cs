using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransaction
    {
        string TransactionId { get; set; }
        string WalletAddress { get; set; }
        string[] SourceWalletAddresses { get; set; }
        double Amount { get; set; }
        string AssetId { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        string Blockchain { get; set; }
        DateTime? FirstSeen { get; set; }
        DateTime? DueDate { get; set; } // When not set, we should use PaymentRequest.DueDate in code.
        TransactionType Type { get; set; }
    }
}
