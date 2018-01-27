namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// The payment request statuses.
    /// </summary>
    public enum PaymentRequestStatus
    {
        /// <summary>
        /// Unknown status.
        /// </summary>
        None,
        
        /// <summary>
        /// Payment request created no transactions.
        /// </summary>
        New,
        
        /// <summary>
        /// Payment request have at least one transaction.
        /// </summary>
        InProcess,
        
        /// <summary>
        /// Payment request have minimum number of transaction confiramtions.
        /// </summary>
        Confirmed,
        
        /// <summary>
        /// An error occurred during processing payment request.
        /// </summary>
        Error
    }
}
