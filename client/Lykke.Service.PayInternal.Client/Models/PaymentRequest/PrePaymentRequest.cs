namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Prepayment request details
    /// </summary>
    public class PrePaymentRequest
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
