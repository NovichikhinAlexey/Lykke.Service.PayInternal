namespace Lykke.Service.PayInternal.Contract.PaymentRequest
{
    /// <summary>
    /// Payment request statuses being used for public APIs
    /// </summary>
    public static class PaymentRequestPublicStatuses
    {
        /// <summary>
        /// Payment request has been created
        /// </summary>
        public const string PaymentRequestCreated = "PAYMENT_REQUEST_CREATED";

        /// <summary>
        /// Payment has been confirmed
        /// </summary>
        public const string PaymentConfirmed = "PAYMENT_CONFIRMED";

        /// <summary>
        /// Payment is in progress
        /// </summary>
        public const string PaymentInProgress = "PAYMENT_INPROGRESS";

        /// <summary>
        /// Payment error
        /// </summary>
        public const string PaymentError = "PAYMENT_ERROR";


        /// <summary>
        /// Refund is in progress
        /// </summary>
        public const string RefundInProgress = "REFUND_INPROGRESS";

        /// <summary>
        /// Refund has been confirmed
        /// </summary>
        public const string RefundConfirmed = "REFUND_CONFIRMED";

        /// <summary>
        /// Refund error
        /// </summary>
        public const string RefundError = "REFUND_ERROR";
    }
}
