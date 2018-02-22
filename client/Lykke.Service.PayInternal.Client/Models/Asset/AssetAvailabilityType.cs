namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    /// <summary>
    /// Asset availability type
    /// </summary>
    public enum AssetAvailabilityType
    {
        /// <summary>
        /// Asset is available to be used as payment asset
        /// </summary>
        Payment,

        /// <summary>
        /// Asset is available to be used as settlement asset
        /// </summary>
        Settlement
    }
}
