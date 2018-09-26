namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    /// <summary>
    /// Payment request error types
    /// </summary>
    public enum PaymentRequestProcessingError
    {
        /// <summary>
        /// Default value, no errors
        /// </summary>
        None = 0,

        /// <summary>
        /// Unexpected unknown error occured during refund
        /// </summary>
        UnknownRefund,

        /// <summary>
        /// Unexpected unknown error occured during payment
        /// </summary>
        UnknownPayment,

        /// <summary>
        /// Amount paid is more than required
        /// </summary>
        PaymentAmountAbove,

        /// <summary>
        /// Amount paid is less than required
        /// </summary>
        PaymentAmountBelow,

        /// <summary>
        /// Payment was not made till the payment request's due date
        /// </summary>
        PaymentExpired,

        /// <summary>
        /// Refund was not confirmed before expiration date
        /// </summary>
        RefundNotConfirmed,

        /// <summary>
        /// Payment was made after the payment request's due date
        /// </summary>
        LatePaid,

        /// <summary>
        /// Unknown error was occured on settlement.
        /// </summary>
        SettlementUnknown,

        /// <summary>
        /// Trading bot balance is less then required for settlement exchange.
        /// </summary>
        SettlementLowBalanceForExchange,

        /// <summary>
        /// Trading bot balance is less then required for settlement transfer to merchant.
        /// </summary>
        SettlementLowBalanceForTransferToMerchant,

        /// <summary>
        /// Payment request's merchant is not found.
        /// </summary>
        SettlementMerchantNotFound,

        /// <summary>
        /// There is no enoung liquidity on Lykke Exchange for settlement exchange.
        /// </summary>
        SettlementNoLiquidityForExchange,

        /// <summary>
        /// Market order leads to negative spread for settlement exchange.
        /// </summary>
        SettlementExchangeLeadToNegativeSpread,

        /// <summary>
        /// Can not retrive transaction details about transfer to settlement trading bot.
        /// </summary>
        SettlementNoTransactionDetails,
    }
}
