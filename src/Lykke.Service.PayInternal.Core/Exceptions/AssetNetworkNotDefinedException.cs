using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetNetworkNotDefinedException : Exception
    {
        public AssetNetworkNotDefinedException()
        {
        }

        public AssetNetworkNotDefinedException(string assetId) : base("Blockchain network is not defined")
        {
            AssetId = assetId;
        }

        public AssetNetworkNotDefinedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetNetworkNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string AssetId { get; set; }
    }
}
