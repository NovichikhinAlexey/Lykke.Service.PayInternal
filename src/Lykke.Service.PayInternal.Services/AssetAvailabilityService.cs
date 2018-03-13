using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using System.Linq;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetAvailabilityService : IAssetsAvailabilityService
    {
        private readonly IAssetAvailabilityRepository _assetAvailabilityRepository;
        private readonly IAssetAvailabilityByMerchantRepository _assetAvailabilityByMerchantRepository;
        private readonly AssetsAvailability _assetsAvailability;

        public AssetAvailabilityService(IAssetAvailabilityRepository assetAvailabilityRepository, 
            IAssetAvailabilityByMerchantRepository assetAvailabilityByMerchantRepository,
            AssetsAvailability assetsAvailability)
        {
            _assetAvailabilityRepository = assetAvailabilityRepository ??
                                           throw new ArgumentNullException(nameof(assetAvailabilityRepository));
            _assetAvailabilityByMerchantRepository = assetAvailabilityByMerchantRepository ??
                                           throw new ArgumentNullException(nameof(assetAvailabilityByMerchantRepository));
            _assetsAvailability = assetsAvailability;
        }
        public async Task<IReadOnlyList<IAssetAvailability>> GetAssetsAvailabilityFromSettings(AssetAvailabilityType assetAvailability)
        {
            var global = await GetAvailableAsync(assetAvailability);
            var assetsFromSettings = new List<IAssetAvailability>();
            if (_assetsAvailability == null)
                return global;
            switch(assetAvailability)
            {
                case AssetAvailabilityType.Payment:
                    {
                        var assets = _assetsAvailability.PaymentAssets.Split(';');
                        foreach(var asset in assets)
                        {
                            var globalasset = global.FirstOrDefault(g => g.AssetId == asset);
                            if (globalasset != null)
                                assetsFromSettings.Add(globalasset);
                        }
                        break;
                    }
                case AssetAvailabilityType.Settlement:
                    {
                        var assets = _assetsAvailability.SettlementAssets.Split(';');
                        foreach (var asset in assets)
                        {
                            var globalasset = global.FirstOrDefault(g => g.AssetId == asset);
                            if (globalasset != null)
                                assetsFromSettings.Add(globalasset);
                        }
                        break;
                    }
            }
            return assetsFromSettings;
        }
        public async Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(AssetAvailabilityType assetAvailability)
        {
            return await _assetAvailabilityRepository.GetAsync(assetAvailability);
        }
        public async Task<IAssetAvailabilityByMerchant> GetAvailablePersonalAsync(string merchantId)
        {
            return await _assetAvailabilityByMerchantRepository.GetAsync(merchantId);
        }
        public async Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(string merchantId, AssetAvailabilityType assetAvailabilityType)
        {
            var assets = await _assetAvailabilityByMerchantRepository.GetAsync(merchantId);
            var globalassets = await GetAvailableAsync(assetAvailabilityType);
            var assetsAvailability = new List<IAssetAvailability>();
            if (assets != null)
            {
                switch(assetAvailabilityType)
                {
                    case AssetAvailabilityType.Payment:
                        {
                            var list = assets.AssetsPayment.Split(';');
                            foreach (var item in list)
                            {
                                var global = globalassets.FirstOrDefault(g => g.AssetId == item);
                                if (global != null)
                                    assetsAvailability.Add(global);
                            }
                            break;
                        }
                    case AssetAvailabilityType.Settlement:
                        {
                            var list = assets.AssetsSettlement.Split(';');
                            foreach (var item in list)
                            {
                                var global = globalassets.FirstOrDefault(g => g.AssetId == item);
                                if (global != null)
                                    assetsAvailability.Add(global);
                            }
                            break;
                        }
                }
                return assetsAvailability;
            }
            return null;
        }

        public async Task<IAssetAvailability> UpdateAsync(string assetId, AssetAvailabilityType assetAvailability,
            bool value)
        {
            return await _assetAvailabilityRepository.SetAsync(assetId, assetAvailability, value);
        }

        public async Task<IAssetAvailabilityByMerchant> UpdateMerchantAssetsAsync(string paymentAssets, string settlementAssets, string merchantId)
        {
            return await _assetAvailabilityByMerchantRepository.SetAsync(paymentAssets, settlementAssets, merchantId);
        }
    }
}
