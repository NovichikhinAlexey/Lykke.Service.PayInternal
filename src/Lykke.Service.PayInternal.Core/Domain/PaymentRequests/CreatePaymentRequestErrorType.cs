using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    /// <summary>
    /// Errors when creating new payment request
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
