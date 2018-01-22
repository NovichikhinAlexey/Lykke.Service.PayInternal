using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransaction
    {
        string Id { get; }
        string TransactionId { get; set; }
        string OrderId { get; set; }
        double Amount { get; set; }
        string BlockId { get; set; }
        int Confirmations { get; set; }
        string WalletAddress { get; set; }
        DateTime FirstSeen { get; set; }
    }
}
