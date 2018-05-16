using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class TransactionExpiredRequest
    {
        [Required]
        public BlockchainType Blockchain { get; set; }

        [Required]
        public TransactionIdentityType IdentityType { get; set; }

        [Required]
        public string Identity { get; set; }
    }
}
