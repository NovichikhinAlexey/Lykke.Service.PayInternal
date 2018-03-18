using System;
using System.Runtime.Serialization;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class NotAllowedStatusException : Exception
    {
        public NotAllowedStatusException()
        {
        }

        public NotAllowedStatusException(string message) : base(message)
        {
        }

        public NotAllowedStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAllowedStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NotAllowedStatusException(PaymentRequestStatus status) : base("Not allowed status")
        {
            Status = status;
        }

        public PaymentRequestStatus Status { get; set; }
    }
}
