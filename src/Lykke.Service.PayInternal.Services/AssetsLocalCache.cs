using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetsLocalCache : IAssetsLocalCache
    {
        private readonly CachedDataDictionary<string, Asset> _assetsCache;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairsCache;

        public AssetsLocalCache(
            CachedDataDictionary<string, Asset> assetsCache,
            CachedDataDictionary<string, AssetPair> assetPairsCache)
        {
            _assetsCache = assetsCache ?? throw new ArgumentNullException(nameof(assetsCache));
            _assetPairsCache = assetPairsCache ?? throw new ArgumentNullException(nameof(assetPairsCache));
        }

        public async Task<Asset> GetAssetByIdAsync(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return null;

            var cachedValues = await _assetsCache.Values();

            return cachedValues.FirstOrDefault(x => x.Id == assetId);
        }

        public async Task<AssetPair> GetAssetPairAsync(string baseAssetId, string quotingAssetId)
        {
            if (string.IsNullOrEmpty(baseAssetId) || string.IsNullOrEmpty(quotingAssetId)) return null;
            
            var cachedValues = await _assetPairsCache.Values();

            return cachedValues.FirstOrDefault(x => x.BaseAssetId == baseAssetId && x.QuotingAssetId == quotingAssetId);
        }
        
        public async Task<AssetPair> GetAssetPairByIdAsync(string assetPairId)
        {
            if (string.IsNullOrEmpty(assetPairId)) return null;

            var cachedValues = await _assetPairsCache.Values();

            return cachedValues.FirstOrDefault(x => x.Id == assetPairId);
        }
    }
}
