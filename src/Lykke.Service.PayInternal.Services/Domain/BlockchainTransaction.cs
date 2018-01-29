using System;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class BlockchainTransaction : IBlockchainTransaction
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public string PaymentRequestId { get; set; }
        public decimal Amount { get; set; }
        public string BlockId { get; set; }
        public int Confirmations { get; set; }
        public string WalletAddress { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}
