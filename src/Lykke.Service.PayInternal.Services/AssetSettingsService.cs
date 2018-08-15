using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetSettingsService : IAssetSettingsService
    {
        private readonly IAssetGeneralSettingsRepository _assetGeneralSettingsRepository;
        private readonly IAssetMerchantSettingsRepository _assetMerchantSettingsRepository;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly IMarkupService _markupService;

        private const char AssetsSeparator = ';';

        public AssetSettingsService(
            [NotNull] IAssetGeneralSettingsRepository assetGeneralAvailabilityRepository,
            [NotNull] IAssetMerchantSettingsRepository assetPersonalAvailabilityRepository,
            [NotNull] IMarkupService markupService, 
            [NotNull] IAssetsLocalCache assetsLocalCache)
        {
            _assetGeneralSettingsRepository = assetGeneralAvailabilityRepository ?? throw new ArgumentNullException(nameof(assetGeneralAvailabilityRepository));
            _assetMerchantSettingsRepository = assetPersonalAvailabilityRepository ?? throw new ArgumentNullException(nameof(assetPersonalAvailabilityRepository));
            _markupService = markupService ?? throw new ArgumentNullException(nameof(markupService));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
        }

        public Task<IReadOnlyList<string>> ResolveSettlementAsync(string merchantId)
        {
            return ResolveAsync(merchantId, AssetAvailabilityType.Settlement);
        }

        public async Task<IReadOnlyList<string>> ResolvePaymentAsync(string merchantId, string settlementAssetId)
        {
            IReadOnlyList<string> assets = await ResolveAsync(merchantId, AssetAvailabilityType.Payment);

            var result = new List<string>();

            foreach (string assetId in assets)
            {
                string assetPairId = $"{assetId}{settlementAssetId}";

                try
                {
                    _markupService.ResolveAsync(merchantId, assetPairId).GetAwaiter().GetResult();

                    result.Add(assetId);
                }
                catch (MarkupNotFoundException)
                {
                }
            }

            return result;
        }

        public async Task<IAssetGeneralSettings> GetGeneralAsync(string assetId)
        {
            string assetIdAdjusted = assetId == LykkeConstants.SatoshiAsset ? LykkeConstants.BitcoinAsset : assetId;

            string assetDisplayId = assetIdAdjusted.IsGuid()
                ? (await _assetsLocalCache.GetAssetByIdAsync(assetIdAdjusted)).DisplayId
                : assetIdAdjusted;

            return await _assetGeneralSettingsRepository.GetAsync(assetDisplayId);
        }

        public async Task<BlockchainType> GetNetworkAsync(string assetId)
        {
            IAssetGeneralSettings assetAvailability = await GetGeneralAsync(assetId);

            return assetAvailability?.Network ?? throw new AssetNetworkNotDefinedException(assetId);
        }

        public async Task<IAssetMerchantSettings> GetByMerchantAsync(string merchantId)
        {
            return await _assetMerchantSettingsRepository.GetAsync(merchantId);
        }

        public async Task<IAssetMerchantSettings> SetByMerchantAsync(string merchantId, string paymentAssets,
            string settlementAssets)
        {
            return await _assetMerchantSettingsRepository.SetAsync(paymentAssets, settlementAssets, merchantId);
        }

        public async Task<IReadOnlyList<IAssetGeneralSettings>> GetGeneralAsync(AssetAvailabilityType type)
        {
            return await _assetGeneralSettingsRepository.GetAsync(type);
        }

        public async Task<IReadOnlyList<IAssetGeneralSettings>> GetGeneralAsync()
        {
            return await _assetGeneralSettingsRepository.GetAsync();
        }

        public async Task<IAssetGeneralSettings> SetGeneralAsync(IAssetGeneralSettings availability)
        {
            return await _assetGeneralSettingsRepository.SetAsync(availability);
        }

        public async Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type)
        {
            IAssetMerchantSettings personalSettings = await _assetMerchantSettingsRepository.GetAsync(merchantId);

            string personalAllowed;

            switch (type)
            {
                case AssetAvailabilityType.Payment:
                    personalAllowed = personalSettings?.PaymentAssets;
                    break;
                case AssetAvailabilityType.Settlement:
                    personalAllowed = personalSettings?.SettlementAssets;
                    break;
                default:
                    throw new Exception("Unexpected asset availability type");
            }

            IEnumerable<string> generalAllowed = (await _assetGeneralSettingsRepository.GetAsync(type)).Select(x => x.AssetId);

            if (string.IsNullOrEmpty(personalAllowed))
                return generalAllowed.ToList();

            IEnumerable<string> resolved =
                personalAllowed.Split(AssetsSeparator).Where(x => generalAllowed.Contains(x));

            return resolved.ToList();
        }
    }
}
