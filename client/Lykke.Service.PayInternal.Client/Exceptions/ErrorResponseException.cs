using System;
using System.Net;
using Common;
using Refit;

namespace Lykke.Service.PayInternal.Client.Exceptions
{
    /// <summary>
    /// Represents base error response from the PayInternal API service
    /// </summary>
    public class ErrorResponseException<T> : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ErrorResponseException{T}"/> with error message.
        /// </summary>
        /// <param name="message">The error message</param>
        public ErrorResponseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ErrorResponseException{T}"/> with response error details and API excepiton.
        /// </summary>
        /// <param name="error">The response error details</param>
        /// <param name="inner">The exception occurred during calling service API.</param>
        public ErrorResponseException(T error, ApiException inner)
            : base(inner.Message ?? string.Empty, inner)
        {
            Error = error;
            StatusCode = inner.StatusCode;
        }

        /// <summary>
        /// Gets a response error details.
        /// </summary>
        public T Error { get; }

        /// <summary>
        /// Gets a http response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        public override string ToString()
        {            
            return $"Error is {Error.ToJson()}\r\n{base.ToString()}";
        }
    }
}
