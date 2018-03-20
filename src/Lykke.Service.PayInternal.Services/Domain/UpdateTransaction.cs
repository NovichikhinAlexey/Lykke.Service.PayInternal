﻿using System;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class UpdateTransaction : IUpdateTransaction
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public double Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime? FirstSeen { get; set; }
    }
}
