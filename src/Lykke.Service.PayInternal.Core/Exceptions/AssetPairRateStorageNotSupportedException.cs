using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetPairRateStorageNotSupportedException : Exception
    {
        public AssetPairRateStorageNotSupportedException()
        {
        }

        public AssetPairRateStorageNotSupportedException(string baseAssetId, string quotingAssetId) : base("The asset pair rate storage not supported")
        {
            BaseAssetId = baseAssetId;
            QuotingAssetId = quotingAssetId;
        }

        public AssetPairRateStorageNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetPairRateStorageNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
    }
}
