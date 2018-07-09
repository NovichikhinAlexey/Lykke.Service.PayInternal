using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    /// <summary>
    /// The type of transaction
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionType
    {
        /// <summary>
        /// Represents payment transaction (created within payment request)
        /// </summary>
        Payment = 0,

        /// <summary>
        /// Represents refund
        /// </summary>
        Refund,

        /// <summary>
        /// Settlement transaction
        /// </summary>
        Settlement,

        /// <summary>
        /// Cash in transaction
        /// </summary>
        CashIn,

        /// <summary>
        /// Exchange transaction
        /// </summary>
        Exchange,

        /// <summary>
        /// Cash out transaction
        /// </summary>
        CashOut
    }
}
