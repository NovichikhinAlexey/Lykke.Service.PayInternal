using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class UpdateTransactionRequest : IUpdateTransactionRequest
    {
        public string Hash { get; set; }

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int Confirmations { get; set; }

        public string BlockId { get; set; }

        public DateTime? FirstSeen { get; set; }

        [Required]
        public TransactionIdentityType IdentityType { get; set; }

        [Required]
        public string Identity { get; set; }
    }
}
