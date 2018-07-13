using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Polly;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetsLocalCache : IAssetsLocalCache
    {
        private readonly CachedDataDictionary<string, Asset> _assetsCache;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairsCache;

        public AssetsLocalCache(
            [NotNull] ILogFactory logFactory,
            [NotNull] IAssetsService assetsService, 
            [NotNull] RetryPolicySettings retryPolicySettings, 
            [NotNull] ExpirationPeriodsSettings expirationPeriodsSettings)
        {
            ILog log = logFactory.CreateLog(this);

            _assetsCache = new CachedDataDictionary<string, Asset>
            (
                async () =>
                {
                    IList<Asset> assets = await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(
                           retryPolicySettings.DefaultAttempts,
                            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                            (ex, timestamp) => log.Error("Getting assets dictionary with retry", ex))
                        .ExecuteAsync(() => assetsService.AssetGetAllAsync(true));

                    return assets.ToDictionary(itm => itm.Id);
                }, expirationPeriodsSettings.AssetsCache);

            _assetPairsCache = new CachedDataDictionary<string, AssetPair>(
                async () =>
                {
                    IList<AssetPair> assetPairs = await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(
                            retryPolicySettings.DefaultAttempts,
                            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                            (ex, timestamp) => log.Error("Getting asset pairs dictionary with retry", ex))
                        .ExecuteAsync(() => assetsService.AssetPairGetAllAsync());

                    return assetPairs.ToDictionary(itm => itm.Id);
                }, expirationPeriodsSettings.AssetsCache);
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
