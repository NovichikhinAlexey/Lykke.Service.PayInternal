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
                case LykkeConstants.BitcoinAsset:
                case LykkeConstants.SatoshiAsset:
                    blockchainType = BlockchainType.Bitcoin;
                    break;
                case LykkeConstants.Erc20PktAsset:
                    blockchainType = BlockchainType.Ethereum;
                    break;
                default: throw new AssetNotSupportedException(assetId);
            }

            return blockchainType;
        }
    }
}
