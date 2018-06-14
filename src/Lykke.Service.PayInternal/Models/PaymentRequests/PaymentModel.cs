using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    /// <summary>
    /// Payment details
    /// </summary>
    public class PaymentModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets payment request id
        /// </summary>
        [Required]
        [RowKey]
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [Required]
        public decimal Amount { get; set; }
    }
}
