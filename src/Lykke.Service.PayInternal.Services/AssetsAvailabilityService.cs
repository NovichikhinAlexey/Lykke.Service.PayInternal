using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetsAvailabilityService : IAssetsAvailabilityService
    {
        private readonly IAssetGeneralAvailabilityRepository _assetGeneralAvailabilityRepository;

        private readonly IAssetPersonalAvailabilityRepository _assetPersonalAvailabilityRepository;

        private readonly AssetsAvailabilitySettings _assetsAvailabilitySettings;

        private const char AssetsSeparator = ';';

        public AssetsAvailabilityService(
            IAssetGeneralAvailabilityRepository assetGeneralAvailabilityRepository, 
            IAssetPersonalAvailabilityRepository assetPersonalAvailabilityRepository,
            AssetsAvailabilitySettings assetsAvailabilitySettings)
        {
            _assetGeneralAvailabilityRepository = assetGeneralAvailabilityRepository ??
                                           throw new ArgumentNullException(nameof(assetGeneralAvailabilityRepository));
            _assetPersonalAvailabilityRepository = assetPersonalAvailabilityRepository ??
                                           throw new ArgumentNullException(nameof(assetPersonalAvailabilityRepository));
            _assetsAvailabilitySettings = assetsAvailabilitySettings ??
                                          throw new ArgumentNullException(nameof(assetsAvailabilitySettings));
        }

        public async Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type)
        {
            IAssetAvailabilityByMerchant personal = await _assetPersonalAvailabilityRepository.GetAsync(merchantId);

            string allowedAssets;

            switch (type)
            {
                case AssetAvailabilityType.Payment:
                    allowedAssets = personal?.PaymentAssets ?? _assetsAvailabilitySettings.PaymentAssets;
                    break;
                case AssetAvailabilityType.Settlement:
                    allowedAssets = personal?.SettlementAssets ?? _assetsAvailabilitySettings.SettlementAssets;
                    break;
                default:
                    throw new Exception("Unexpected asset availability type");
            }

            IEnumerable<string> generalAllowed =
                (await _assetGeneralAvailabilityRepository.GetAsync(type)).Select(x => x.AssetId);

            IEnumerable<string> resolved = allowedAssets.Split(AssetsSeparator).Where(x => generalAllowed.Contains(x));

            return resolved.ToList();
        }

        public async Task<IAssetAvailabilityByMerchant> GetPersonalAsync(string merchantId)
        {
            return await _assetPersonalAvailabilityRepository.GetAsync(merchantId);
        }

        public async Task<IAssetAvailabilityByMerchant> SetPersonalAsync(string merchantId, string paymentAssets, string settlementAssets)
        {
            return await _assetPersonalAvailabilityRepository.SetAsync(paymentAssets, settlementAssets, merchantId);
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetGeneralByTypeAsync(AssetAvailabilityType type)
        {
            return await _assetGeneralAvailabilityRepository.GetAsync(type);
        }

        public async Task<IAssetAvailability> SetGeneralAsync(string assetId, AssetAvailabilityType type, bool value)
        {
            return await _assetGeneralAvailabilityRepository.SetAsync(assetId, type, value);
        }
    }
}
