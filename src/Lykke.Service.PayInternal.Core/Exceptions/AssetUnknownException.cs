using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetUnknownException : Exception
    {
        public AssetUnknownException()
        {
        }

        public AssetUnknownException(string asset) : base("Unknown asset id")
        {
            Asset = asset;
        }

        public AssetUnknownException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetUnknownException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Asset { get; set; }
    }
}
