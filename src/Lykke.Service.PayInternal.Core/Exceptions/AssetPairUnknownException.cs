using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetPairUnknownException : Exception
    {
        public AssetPairUnknownException()
        {
        }

        public AssetPairUnknownException(string baseAssetId, string quotingAssetId) : base("Unknown asset pair id")
        {
            BaseAssetId = baseAssetId;
            QuotingAssetId = quotingAssetId;
        }

        public AssetPairUnknownException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetPairUnknownException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
    }
}
