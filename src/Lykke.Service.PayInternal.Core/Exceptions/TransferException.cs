using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class TransferException : Exception
    {
        public TransferException()
        {
        }

        public TransferException(string message) : base(message)
        {
        }

        public TransferException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransferException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransferException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}
