using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetAvailabilityService : IAssetsAvailabilityService
    {
        private readonly IAssetAvailabilityRepository _assetAvailabilityRepository;

        public AssetAvailabilityService(IAssetAvailabilityRepository assetAvailabilityRepository)
        {
            _assetAvailabilityRepository = assetAvailabilityRepository ??
                                           throw new ArgumentNullException(nameof(assetAvailabilityRepository));
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(AssetAvailabilityType assetAvailability)
        {
            return await _assetAvailabilityRepository.GetAsync(assetAvailability);
        }

        public async Task<IAssetAvailability> UpdateAsync(string assetId, AssetAvailabilityType assetAvailability,
            bool value)
        {
            return await _assetAvailabilityRepository.SetAsync(assetId, assetAvailability, value);
        }
    }
}
