using System;

namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class CreateTransactionRequest
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public string[] SourceWalletAddresses { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public BlockchainType Blockchain { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}
