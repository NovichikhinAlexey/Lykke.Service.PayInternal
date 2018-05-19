using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class CreateLykkeTransactionRequest : ICreateLykkeTransactionRequest
    {
        [Required]
        public string OperationId { get; set; }

        [Required]
        public string[] SourceWalletAddresses { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string AssetId { get; set; }

        [Required]
        public int Confirmations { get; set; }

        [Required]
        public TransactionIdentityType IdentityType { get; set; }

        [Required]
        public string Identity { get; set; }
    }
}
