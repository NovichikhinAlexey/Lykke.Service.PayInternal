using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetsAvailabilityService : IAssetsAvailabilityService
    {
        private readonly IAssetGeneralAvailabilityRepository _assetGeneralAvailabilityRepository;
        private readonly IAssetPersonalAvailabilityRepository _assetPersonalAvailabilityRepository;
        private readonly IMarkupService _markupService;

        private const char AssetsSeparator = ';';

        public AssetsAvailabilityService(
            IAssetGeneralAvailabilityRepository assetGeneralAvailabilityRepository,
            IAssetPersonalAvailabilityRepository assetPersonalAvailabilityRepository,
            IMarkupService markupService)
        {
            _assetGeneralAvailabilityRepository = assetGeneralAvailabilityRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(assetGeneralAvailabilityRepository));
            _assetPersonalAvailabilityRepository = assetPersonalAvailabilityRepository ??
                                                   throw new ArgumentNullException(
                                                       nameof(assetPersonalAvailabilityRepository));
            _markupService = markupService ?? throw new ArgumentNullException(nameof(markupService));
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

        public async Task<BlockchainType> GetNetworkAsync(string assetId)
        {
            string assetIdAdjusted = assetId == LykkeConstants.SatoshiAsset ? LykkeConstants.BitcoinAsset : assetId;

            IAssetAvailability assetAvailability = await _assetGeneralAvailabilityRepository.GetAsync(assetIdAdjusted);

            return assetAvailability?.Network ?? throw new Exception($"Blockchain network is not defined for asset [{assetId}]");
        }

        public async Task<IAssetAvailabilityByMerchant> GetPersonalAsync(string merchantId)
        {
            return await _assetPersonalAvailabilityRepository.GetAsync(merchantId);
        }

        public async Task<IAssetAvailabilityByMerchant> SetPersonalAsync(string merchantId, string paymentAssets,
            string settlementAssets)
        {
            return await _assetPersonalAvailabilityRepository.SetAsync(paymentAssets, settlementAssets, merchantId);
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetGeneralByTypeAsync(AssetAvailabilityType type)
        {
            return await _assetGeneralAvailabilityRepository.GetByTypeAsync(type);
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetGeneralAsync()
        {
            return await _assetGeneralAvailabilityRepository.GetAsync();
        }

        public async Task<IAssetAvailability> SetGeneralAsync(IAssetAvailability availability)
        {
            return await _assetGeneralAvailabilityRepository.SetAsync(availability);
        }

        public async Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type)
        {
            IAssetAvailabilityByMerchant personalSettings = await _assetPersonalAvailabilityRepository.GetAsync(merchantId);

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

            IEnumerable<string> generalAllowed = (await _assetGeneralAvailabilityRepository.GetByTypeAsync(type)).Select(x => x.AssetId);

            if (string.IsNullOrEmpty(personalAllowed))
                return generalAllowed.ToList();

            IEnumerable<string> resolved =
                personalAllowed.Split(AssetsSeparator).Where(x => generalAllowed.Contains(x));

            return resolved.ToList();
        }
    }
}
