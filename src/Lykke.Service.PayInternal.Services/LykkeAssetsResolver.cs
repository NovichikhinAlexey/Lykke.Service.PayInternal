using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class LykkeAssetsResolver : ILykkeAssetsResolver
    {
        private readonly AssetsMapSettings _assetsMapSettings;
        private readonly IAssetsLocalCache _assetsLocalCache;

        public LykkeAssetsResolver(
            AssetsMapSettings assetsMapSettings, 
            IAssetsLocalCache assetsLocalCache)
        {
            _assetsMapSettings = assetsMapSettings;
            _assetsLocalCache = assetsLocalCache;
        }

        public async Task<string> GetLykkeId(string assetId)
        {
            Asset asset = await _assetsLocalCache.GetAssetByIdAsync(assetId);

            if (asset != null)
                return asset.Id;

            if (_assetsMapSettings.Values.TryGetValue(assetId, out string lykkeId))
            {
                return lykkeId;
            }

            throw new AssetUnknownException(assetId);
        }
    }
}
