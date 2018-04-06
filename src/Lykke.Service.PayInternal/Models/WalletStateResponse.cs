﻿using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models
{
    public class WalletStateResponse
    {
        public string Address { get; set; }
        public DateTime DueDate { get; set; }
        public IEnumerable<PayTransactionStateResponse> Transactions { get; set; }
    }

    public class PayTransactionStateResponse
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockId { get; set; }
        public BlockchainType Blockchain { get; set; }
        public int Confirmations { get; set; }
        public string WalletAddress { get; set; }
    }
}
