using System;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models.Transactions
{
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
