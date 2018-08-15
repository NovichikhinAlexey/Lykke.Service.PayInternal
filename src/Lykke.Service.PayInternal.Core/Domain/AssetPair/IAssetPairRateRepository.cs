using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.AssetPair
{
    public interface IAssetPairRateRepository
    {
        Task<IAssetPairRate> AddAsync(IAssetPairRate src);

        Task<IReadOnlyList<IAssetPairRate>> GetByDateAsync(DateTime date);

        Task<IReadOnlyList<IAssetPairRate>> GetAsync(string baseAssetId, string quotingAssetId);
    }
}
