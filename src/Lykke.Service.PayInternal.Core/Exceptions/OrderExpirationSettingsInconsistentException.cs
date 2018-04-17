using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class OrderExpirationSettingsInconsistentException : Exception
    {
        public OrderExpirationSettingsInconsistentException()
        {
        }

        public OrderExpirationSettingsInconsistentException(string message) : base(message)
        {
        }

        public OrderExpirationSettingsInconsistentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrderExpirationSettingsInconsistentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public OrderExpirationSettingsInconsistentException(TimeSpan primary, TimeSpan extended) : base("Extended order expiration period has to be larger than primary")
        {
            Primary = primary;
            Extended = extended;
        }

        public TimeSpan Primary { get; set; }

        public TimeSpan Extended { get; set; }
    }
}
