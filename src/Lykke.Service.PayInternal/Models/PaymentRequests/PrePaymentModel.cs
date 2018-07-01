using Lykke.Service.PayInternal.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    /// <summary>
    /// Prepayment details
    /// </summary>
    public class PrePaymentModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets payer merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string PayerMerchantId { get; set; }

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
