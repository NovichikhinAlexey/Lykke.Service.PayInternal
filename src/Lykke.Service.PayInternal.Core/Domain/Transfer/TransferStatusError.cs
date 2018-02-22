namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Transfer status error. If Transfer status is error, the property describe why. If status is not Error - NotError present.
    /// </summary>
    public enum TransferStatusError
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
        InternalError,
        /// <summary>
        /// The requested merchand does not exist.
        /// </summary>
        MerchantNotFound,
        /// <summary>
        /// The requested merchand does not have any wallets.
        /// </summary>
        MerchantHasNoWallets
    }
}
