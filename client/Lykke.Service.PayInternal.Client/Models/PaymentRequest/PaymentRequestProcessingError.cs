namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Payment request error types
    /// </summary>
    public enum PaymentRequestProcessingError
    {
        /// <summary>
        /// Default value, no errors
        /// </summary>
        None = 0,

        /// <summary>
        /// Unexpected unknown error occured during refund
        /// </summary>
        UnknownRefund,

        /// <summary>
        /// Unexpected unknown error occured during payment
        /// </summary>
        UnknownPayment,

        /// <summary>
        /// Amount paid is more than required
        /// </summary>
        PaymentAmountAbove,

        /// <summary>
        /// Amount paid is less than required
        /// </summary>
        PaymentAmountBelow,

        /// <summary>
        ///  Payment was not made till the payment request's due date
        /// </summary>
        PaymentExpired,

        /// <summary>
        /// Refund was not confirmed before expiration date
        /// </summary>
        RefundNotConfirmed,

        /// <summary>
        /// Payment was made after the payment request's due date
        /// </summary>
        LatePaid
    }
}
