using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Refit;

namespace Lykke.Service.PayInternal.Client.Exceptions
{
    /// <summary>
    /// Represents refund error response from the PayInternal API service
    /// </summary>
    public class RefundErrorResponseException : ErrorResponseException<RefundError>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RefundErrorResponseException"/> with error message.
        /// </summary>
        /// <param name="message"></param>
        public RefundErrorResponseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RefundErrorResponseException"/> with response error details and API excepiton.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="inner"></param>
        public RefundErrorResponseException(RefundError error, ApiException inner) : base(error, inner)
        {
        }
    }
}
