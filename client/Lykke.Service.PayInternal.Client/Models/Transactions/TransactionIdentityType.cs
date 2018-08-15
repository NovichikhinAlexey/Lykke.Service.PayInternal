namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    /// <summary>
    /// The identity type of transaction
    /// </summary>
    public enum TransactionIdentityType
    {
        /// <summary>
        /// Transaction is identified by blockchain hash
        /// </summary>
        Hash = 0,

        /// <summary>
        /// The transaction identification is specific to implementation
        /// </summary>
        Specific
    }
}
