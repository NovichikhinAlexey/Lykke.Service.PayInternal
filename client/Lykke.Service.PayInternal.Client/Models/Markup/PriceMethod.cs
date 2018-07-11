using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models.Markup
{
    /// <summary>
    /// Price determination method
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceMethod
    {
        /// <summary>
        /// Not set
        /// </summary>
        None = 0,

        /// <summary>
        /// Direct, the market price will be used
        /// </summary>
        Direct,

        /// <summary>
        /// Reverse, the reverted price will be used
        /// </summary>
        Reverse
    }
}
