using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetGeneralAvailabilityRepository
    {
        Task<IReadOnlyList<IAssetAvailability>> GetByTypeAsync(AssetAvailabilityType availabilityType);

        Task<IReadOnlyList<IAssetAvailability>> GetAsync();

        Task<IAssetAvailability> GetAsync(string assetId);

        Task<IAssetAvailability> SetAsync(IAssetAvailability availability);
    }
}
