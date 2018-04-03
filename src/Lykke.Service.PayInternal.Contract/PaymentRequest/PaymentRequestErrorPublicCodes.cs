namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    /// <summary>
    /// Payment request error status codes being used for public APIs
    /// </summary>
    public class PaymentRequestErrorPublicCodes
    {
        /// <summary>
        /// The amount paid is more than required
        /// </summary>
        public const string PaymentAmountAbove = "AMOUNT_ABOVE";

        /// <summary>
        /// The amount paid is less than required
        /// </summary>
        public const string PaymentAmountBelow = "AMOUNT_BELOW";

        /// <summary>
        /// Payment request order has been expired
        /// </summary>
        public const string PaymentExpired = "PAYMENT_EXPIRED";

        /// <summary>
        /// Any payment request transaction type has not been confirmed
        /// </summary>
        public const string TransactionNotConfirmed = "TRANSACTION_NOT_CONFIRMED";

        /// <summary>
        /// Any payment request transaction type has not been detected
        /// </summary>
        public const string TransactionNotDetected = "TRANSACTION_NOT_DETECTED";

        /// <summary>
        /// Common refund error, typically means unexpected unknown error
        /// </summary>
        public const string RefundIsNotAvailable = "REFUND_IS_NOT_AVAILABLE";
    }
}
