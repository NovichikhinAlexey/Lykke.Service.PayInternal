namespace Lykke.Service.PayInternal.Core.Domain.Transaction
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
