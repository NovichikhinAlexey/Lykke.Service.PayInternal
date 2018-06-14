using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetPairSettingsService : IAssetPairSettingsService
    {
        private readonly IReadOnlyList<AssetPairSetting> _assetPairLocalStorageSettings;
        private readonly IAssetsLocalCache _assetsLocalCache;

        public AssetPairSettingsService(
            [NotNull] IReadOnlyList<AssetPairSetting> assetPairLocalStorageSettings, 
            [NotNull] IAssetsLocalCache assetsLocalCache)
        {
            _assetPairLocalStorageSettings = assetPairLocalStorageSettings ?? throw new ArgumentNullException(nameof(assetPairLocalStorageSettings));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
        }

        public async Task<bool> Contains(string baseAssetId, string quotingAssetId)
        {
            Asset baseAsset = await _assetsLocalCache.GetAssetByIdAsync(baseAssetId);

            Asset quotingAsset = await _assetsLocalCache.GetAssetByIdAsync(quotingAssetId);

            return _assetPairLocalStorageSettings.Any(x =>
                x.BaseAssetId == baseAsset.DisplayId && x.QuotingAssetId == quotingAsset.DisplayId);
        }
    }
}
