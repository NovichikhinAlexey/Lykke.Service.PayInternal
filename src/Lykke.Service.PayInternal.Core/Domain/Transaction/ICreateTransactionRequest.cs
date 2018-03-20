﻿using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransactionRequest
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
    }
}
