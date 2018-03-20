using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class TransactionExpiredRequest
    {
        [Required]
        public string TransactionId { get; set; }
    }
}
