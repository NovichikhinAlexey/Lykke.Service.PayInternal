using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class TransactionExpiredRequest
    {
        [Required]
        public string TransactionId { get; set; }

        [Required]
        public BlockchainType Blockchain { get; set; }
    }
}
