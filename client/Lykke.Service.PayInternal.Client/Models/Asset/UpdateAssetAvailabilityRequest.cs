namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    /// <summary>
    /// Request to update asset availability
    /// </summary>
    public class UpdateAssetAvailabilityRequest
    {
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Asset availability type
        /// </summary>
        public AssetAvailabilityType AvailabilityType { get; set; }

        /// <summary>
        /// Value for availability type
        /// </summary>
        public bool Value { get; set; }
    }
}
