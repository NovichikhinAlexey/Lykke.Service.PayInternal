using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnrecognizedApiResponse : Exception
    {
        public UnrecognizedApiResponse()
        {
        }

        public UnrecognizedApiResponse(string responseType) : base("Unrecognized API response")
        {
            ResponseType = responseType;
        }

        public UnrecognizedApiResponse(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnrecognizedApiResponse(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string ResponseType { get; set; }
    }
}
