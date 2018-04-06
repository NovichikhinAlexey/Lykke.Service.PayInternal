namespace Lykke.Service.PayInternal.Client.Models.Order
{
    /// <summary>
    /// Represent an order checkout information.
    /// </summary>
    public class ChechoutRequestModel
    {
        /// <summary>
        /// The merchant id.
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// The payment request which should be checkouted.
        /// </summary>
        public string PaymentRequestId { get; set; }
    }
}
