namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Payment details
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets payer merchant id
        /// </summary>
        public string PayerMerchantId { get; set; }

        /// <summary>
        /// Gets or sets payment request id
        /// </summary>
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        public decimal Amount { get; set; }
    }
}
