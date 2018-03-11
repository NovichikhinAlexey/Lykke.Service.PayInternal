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

        public string AssetId { get; set; }

        public string BlockId { get; set; }

        public string Blockchain { get; set; }

        public int Confirmations { get; set; }

        public string WalletAddress { get; set; }

        public DateTime? FirstSeen { get; set; }

        public TransactionType TransactionType { get; set; }

        public DateTime DueDate { get; set; }
    }
}
