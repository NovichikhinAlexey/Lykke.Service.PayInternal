namespace Lykke.Service.PayInternal.Models.Cashout
{
    /// <summary>
    /// Cashout operation result details
    /// </summary>
    public class CashoutResponse
    {
        /// <summary>
        /// Gets or sets source wallet address
        /// </summary>
        public string SourceWalletAddress { get; set; }

        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets destination wallet address
        /// </summary>
        public string DestWalletAddress { get; set; }
    }
}

