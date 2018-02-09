using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransactionRequest : IUpdateTransactionRequest
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int Confirmations { get; set; }

        [Required]
        public string BlockId { get; set; }
    }
}
