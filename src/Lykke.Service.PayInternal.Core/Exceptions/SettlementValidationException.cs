using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class SettlementValidationException : Exception
    {
        public SettlementValidationException()
        {
        }

        public SettlementValidationException(string message) : base(message)
        {
        }

        public SettlementValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SettlementValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
