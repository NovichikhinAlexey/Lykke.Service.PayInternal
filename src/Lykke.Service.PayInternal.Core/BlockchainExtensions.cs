using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.Core
{
    public static class BlockchainExtensions
    {
        public static BlockchainType GetBlockchainType(this string assetId)
        {
            BlockchainType blockchainType;

            switch (assetId)
            {
                case LykkeConstants.BitcoinAssetId:
                case LykkeConstants.SatoshiAssetId:
                    blockchainType = BlockchainType.Bitcoin;
                    break;
                default: throw new AssetNotSupportedException(assetId);
            }

            return blockchainType;
        }
    }
}
