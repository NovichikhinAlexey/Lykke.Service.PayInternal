namespace Lykke.Service.PayInternal.Contract.TransferRequest
{
    public enum TransferStatus
    {
        /// <summary>
        /// No errors. If status of transaction request is not Error.
        /// </summary>
        NotError = 0,
        /// <summary>
        /// Transaction is not confirmed.
        /// </summary>
        NotConfirmed,
        /// <summary>
        /// Show invalid amount. Means source wallets could have not enought amount or amount is not enought to pay fee.
        /// </summary>
        InvalidAmount,
        /// <summary>
        /// Invalid address.
        /// </summary>
        InvalidAddress,
        /// <summary>
        /// Any other errors. Take a look thow the logs.
        /// </summary>
        InternalError
    }
}
