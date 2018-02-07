namespace Lykke.Service.PayInternal.Contract.TransferRequest
{
    public enum TransferStatusError
    {
        /// <summary>
        /// In Progress. The status apply if the transaction was registred in blockchain successful 
        /// </summary>
        InProgress,
        /// <summary>
        /// Completed. The status apply when PRC transaction does have enought comfirmations
        /// </summary>
        Completed,
        /// <summary>
        /// Error. The status apply when somthing wrong happens
        /// </summary>
        Error
    }
}
