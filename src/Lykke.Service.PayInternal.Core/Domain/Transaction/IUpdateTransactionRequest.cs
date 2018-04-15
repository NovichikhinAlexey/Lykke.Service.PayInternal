using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IUpdateTransactionRequest : IWalletAddressHolder
    {
        string TransactionId { get; set; }
        double Amount { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        DateTime? FirstSeen { get; set; }
    }
}
