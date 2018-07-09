using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class CashoutOperationFailedException : Exception
    {
        public CashoutOperationFailedException()
        {
        }

        public CashoutOperationFailedException(IEnumerable<string> transferErrors) : base("Cashout operation failed")
        {
            TransferErrors = transferErrors;
        }

        public CashoutOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CashoutOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> TransferErrors { get; set; }
    }
}
