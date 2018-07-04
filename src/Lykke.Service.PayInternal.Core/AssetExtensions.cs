using System;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;

namespace Lykke.Service.PayInternal.Core
{
    public static class AssetExtensions
    {
        public static AddAssetPairRateCommand Invert(this AddAssetPairRateCommand src)
        {
            return new AddAssetPairRateCommand
            {
                BaseAssetId = src.QuotingAssetId,
                QuotingAssetId = src.BaseAssetId,
                BidPrice = src.BidPrice > 0 ? 1 / src.BidPrice : 0,
                AskPrice = src.AskPrice > 0 ? 1 / src.AskPrice : 0
            };
        }

        public static AddAssetPairRateCommand ApplyAccuracy(this AddAssetPairRateCommand src, int accuracy)
        {
            return new AddAssetPairRateCommand
            {
                BaseAssetId = src.BaseAssetId,
                QuotingAssetId = src.QuotingAssetId,
                BidPrice = Math.Round(src.BidPrice, accuracy),
                AskPrice = Math.Round(src.AskPrice, accuracy)
            };
        }
    }
}
