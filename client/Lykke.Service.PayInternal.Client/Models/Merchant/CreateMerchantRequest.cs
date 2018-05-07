namespace Lykke.Service.PayInternal.Client.Models.Merchant
{
    /// <summary>
    /// The merchant create request.
    /// </summary>
    public class CreateMerchantRequest
    {
        /// <summary>
        /// Gets or sets the merchant name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets ot sets the merchant display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the merchant api key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the merchant time cache rates.
        /// </summary>
        public int TimeCacheRates { get; set; }

        /// <summary>
        /// Gets or sets the merchant Lykke wallet client id.
        /// </summary>
        public string LwId { get; set; }
    }
}
