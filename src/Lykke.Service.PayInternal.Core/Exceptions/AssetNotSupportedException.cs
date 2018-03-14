using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetNotSupportedException : Exception
    {
        public AssetNotSupportedException()
        {
        }

        public AssetNotSupportedException(string assetId) : base("Asset not supported")
        {
            AssetId = assetId;
        }

        public AssetNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string AssetId { get; set; }
    }
}
