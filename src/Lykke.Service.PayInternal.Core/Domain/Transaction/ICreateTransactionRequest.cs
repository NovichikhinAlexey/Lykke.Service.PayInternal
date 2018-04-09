using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransactionRequest : IWalletAddressHolder
    {
        string TransactionId { get; set; }
        string[] SourceWalletAddresses { get; set; } 
        decimal Amount { get; set; }
        string AssetId { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        DateTime? FirstSeen { get; set; }
    }
}
