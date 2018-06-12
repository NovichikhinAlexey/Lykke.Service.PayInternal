namespace Lykke.Service.PayInternal.Client.Models.MerchantGroups
{
    /// <summary>
    /// Get merchants by usage request details
    /// </summary>
    public class GetMerchantsByUsageRequest
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets merchant group use
        /// </summary>
        public MerchantGroupUse MerchantGroupUse { get; set; }
    }
}
