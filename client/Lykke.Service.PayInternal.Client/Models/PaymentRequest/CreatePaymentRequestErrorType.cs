namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Errors when creating new payment request
    /// </summary>
    public enum CreatePaymentRequestErrorType
    {
        /// <summary>
        /// Request model validation error
        /// </summary>
        RequestValidationCommonError = 0,

        /// <summary>
        /// Settlement asset is not available
        /// </summary>
        SettlementAssetNotAvailable,

        /// <summary>
        /// Payment asset is not available
        /// </summary>
        PaymentAssetNotAvailable
    }
}
