using Lykke.Common.Api.Contract.Responses;
using Refit;

namespace Lykke.Service.PayInternal.Client.Exceptions
{
    /// <summary>
    /// Represents error response from the PayInternal API service
    /// </summary>
    public class DefaultErrorResponseException : ErrorResponseException<ErrorResponse>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DefaultErrorResponseException"/> with error message.
        /// </summary>
        /// <param name="message"></param>
        public DefaultErrorResponseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultErrorResponseException"/> with response error details and API excepiton.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="inner"></param>
        public DefaultErrorResponseException(ErrorResponse error, ApiException inner) : base(error, inner)
        {
        }
    }
}
