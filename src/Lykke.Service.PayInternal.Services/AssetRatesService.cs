using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetRatesService : IAssetRatesService
    {
        private readonly IAssetPairRateRepository _assetPairRateRepository;

        public AssetRatesService([NotNull] IAssetPairRateRepository assetPairRateRepository)
        {
            _assetPairRateRepository = assetPairRateRepository ?? throw new ArgumentNullException(nameof(assetPairRateRepository));
        }

        public Task<IAssetPairRate> AddAsync(AddAssetPairRateCommand cmd)
        {
            var newRate = Mapper.Map<AssetPairRate>(cmd);

            return _assetPairRateRepository.AddAsync(newRate);
        }

        public async Task<IAssetPairRate> GetCurrentRate(string baseAssetId, string quotingAssetId)
        {
            IReadOnlyList<IAssetPairRate> allRates =
                await _assetPairRateRepository.GetAsync(baseAssetId, quotingAssetId);

            return allRates
                .Where(x => x.CreatedOn <= DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefault();
        }
    }
}
