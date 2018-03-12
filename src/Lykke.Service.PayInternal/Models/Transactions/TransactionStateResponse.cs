using System;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    // TODO: remove this class after Contract assembly update.
    public class TransactionStateResponse
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockId { get; set; }
        public string Blockchain { get; set; }
        public int Confirmations { get; set; }
        public DateTime DueDate { get; set; }
        public string WalletAddress { get; set; }
    }
}
