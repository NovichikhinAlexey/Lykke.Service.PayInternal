using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetNotSupportedException : Exception
    {
        public AssetNotSupportedException()
        {
        }

        public AssetNotSupportedException(string asset) : base("Asset not supported")
        {
            Asset = asset;
        }

        public AssetNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Asset { get; set; }
    }
}
