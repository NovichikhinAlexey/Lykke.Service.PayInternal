using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models
{
    /// <summary>
    /// Blockchain type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlockchainType
    {
        /// <summary>
        /// Not a blockchain
        /// </summary>
        None = 0,

        /// <summary>
        /// Bitcoin blockchain
        /// </summary>
        Bitcoin,

        /// <summary>
        /// Ethereum blockchain
        /// </summary>
        Ethereum,

        /// <summary>
        /// Ethereum blockchain with IATA specific implementation
        /// </summary>
        EthereumIata
    }
}
