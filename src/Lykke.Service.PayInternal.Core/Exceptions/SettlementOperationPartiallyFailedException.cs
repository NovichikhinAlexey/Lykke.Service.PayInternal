using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class SettlementOperationPartiallyFailedException : SettlementOperationFailedException
    {
        public SettlementOperationPartiallyFailedException()
        {
        }

        public SettlementOperationPartiallyFailedException(IEnumerable<string> transferErrors) : base(transferErrors)
        {
        }

        public SettlementOperationPartiallyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SettlementOperationPartiallyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
