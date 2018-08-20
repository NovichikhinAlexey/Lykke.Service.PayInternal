using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetRatesService
    {
        Task<IAssetPairRate> AddAsync(AddAssetPairRateCommand cmd);

        Task<IAssetPairRate> GetCurrentRateAsync(string baseAssetId, string quotingAssetId);
    }
}
