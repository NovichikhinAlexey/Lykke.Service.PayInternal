using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    [Serializable]
    public class UnexpectedAssetException : Exception
    {
        public UnexpectedAssetException()
        {
        }

        public UnexpectedAssetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedAssetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UnexpectedAssetException(string assetId) : base("Unexpected asset")
        {
            AssetId = assetId;
        }

        public string AssetId { get; set; }
    }
}
