using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class BtcTransferException : Exception
    {
        public BtcTransferException()
        {
        }

        public BtcTransferException(string message) : base(message)
        {
        }

        public BtcTransferException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BtcTransferException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BtcTransferException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}
