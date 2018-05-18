using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class RefundValidationException : Exception
    {
        public RefundValidationException()
        {
        }

        public RefundValidationException(RefundErrorType errorType) : base("Refund input data is invalid")
        {
            ErrorType = errorType;
        }

        public RefundValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RefundValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RefundErrorType ErrorType { get; set; }
    }
}
