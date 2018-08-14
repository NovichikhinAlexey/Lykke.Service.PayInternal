using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Refit;

namespace Lykke.Service.PayInternal.Client.Exceptions
{
    /// <summary>
    /// Represents payment request creation error response from the PayInternal API service
    /// </summary>
    public class CreatePaymentRequestResponseException : ErrorResponseException<CreatePaymentRequestError>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CreatePaymentRequestResponseException"/> with error message.
        /// </summary>
        /// <param name="message"></param>
        public CreatePaymentRequestResponseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CreatePaymentRequestResponseException"/> with response error details and API excepiton.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="inner"></param>
        public CreatePaymentRequestResponseException(CreatePaymentRequestError error, ApiException inner) : base(error, inner)
        {
        }
    }
}
