using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransactionRequest : IUpdateTransactionRequest
    {
        [Required]
        public string TransactionId { get; set; }

        public string WalletAddress { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int Confirmations { get; set; }

        public string BlockId { get; set; }

        public DateTime? FirstSeen { get; set; }
    }
}
