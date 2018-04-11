using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.Wallets
{
    public class WalletStateResponse
    {
        public string Address { get; set; }
        public BlockchainType Blockchain { get; set; }
        public DateTime DueDate { get; set; }
        public IEnumerable<TransactionStateResponse> Transactions { get; set; }
    }

    public class TransactionStateResponse
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockId { get; set; }
        public BlockchainType Blockchain { get; set; }
        public int Confirmations { get; set; }
        public DateTime DueDate { get; set; }
        public string WalletAddress { get; set; }
    }
}
