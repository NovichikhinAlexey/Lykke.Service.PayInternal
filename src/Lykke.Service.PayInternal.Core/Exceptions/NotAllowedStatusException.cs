using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class NotAllowedStatusException : Exception
    {
        public NotAllowedStatusException()
        {
        }

        public NotAllowedStatusException(string status) : base("Not allowed status")
        {
            Status = status;
        }

        public NotAllowedStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAllowedStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Status { get; set; }
    }
}
