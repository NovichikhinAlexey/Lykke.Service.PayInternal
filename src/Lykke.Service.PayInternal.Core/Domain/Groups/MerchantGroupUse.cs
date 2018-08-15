using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Core.Domain.Groups
{
    /// <summary>
    /// The purpose of merchant group
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MerchantGroupUse
    {
        /// <summary>
        /// Merchant group to be supervised
        /// </summary>
        Supervising,

        /// <summary>
        /// Merchant group to bill against
        /// </summary>
        Billing
    }
}
