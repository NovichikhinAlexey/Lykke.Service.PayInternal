﻿using System;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransaction
    {
        string Id { get; }
        string TransactionId { get; set; }
        string PaymentRequestId { get; set; }
        decimal Amount { get; set; }
        string AssetId { get; set; }
        [CanBeNull] string BlockId { get; set; }
        string Blockchain { get; set; }
        int Confirmations { get; set; }
        string WalletAddress { get; set; }
        DateTime? FirstSeen { get; set; }
        string[] SourceWalletAddresses { get; set; }
        TransactionType TransactionType { get; set; }
    }
}
