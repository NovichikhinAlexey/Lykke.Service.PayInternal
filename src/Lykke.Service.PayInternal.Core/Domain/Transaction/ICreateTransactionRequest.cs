using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransactionRequest
    {
        string TransactionId { get; set; }
        string WalletAddress { get; set; }
        string[] SourceWalletAddresses { get; set; } 
        decimal Amount { get; set; }
        string AssetId { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        BlockchainType Blockchain { get; set; }
        DateTime? FirstSeen { get; set; }
    }
}
