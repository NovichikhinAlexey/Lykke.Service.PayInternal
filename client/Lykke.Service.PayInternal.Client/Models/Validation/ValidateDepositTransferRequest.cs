namespace Lykke.Service.PayInternal.Client.Models.Validation
{
    /// <summary>
    /// Request details to validate deposit transfer from temporary addresses
    /// </summary>
    public class ValidateDepositTransferRequest
    {
        /// <summary>
        /// Gets or sets blockchain type
        /// </summary>
        public BlockchainType Blockchain { get; set; }

        /// <summary>
        /// Gets or sets blockchain wallet address
        /// </summary>
        public string WalletAddress { get; set; }

        /// <summary>
        /// Gets or sets deposit transfer amount
        /// </summary>
        public decimal TransferAmount { get; set; }
    }
}
