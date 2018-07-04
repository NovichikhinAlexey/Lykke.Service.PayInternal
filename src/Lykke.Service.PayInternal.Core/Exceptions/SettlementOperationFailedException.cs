using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class SettlementOperationFailedException : Exception
    {
        public SettlementOperationFailedException()
        {
        }

        public SettlementOperationFailedException(IEnumerable<string> transferErrors) : base("Settlement operation failed")
        {
            TransferErrors = transferErrors;
        }

        public SettlementOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SettlementOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> TransferErrors { get; set; }
    }
}
