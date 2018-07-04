namespace Lykke.Service.PayInternal.Client.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class ExchangeRequest: PreExchangeRequest
    {
        /// <summary>
        /// Gets or sets source merchant wallet id
        /// </summary>
        public string SourceMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets destination merchant walletd id
        /// </summary>
        public string DestMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets expected rate
        /// </summary>
        public decimal ExpectedRate { get; set; }
    }
}
