using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetGeneralAvailabilityRepository
    {
        Task<IReadOnlyList<IAssetAvailability>> GetAsync(AssetAvailabilityType availability);

        Task<IAssetAvailability> SetAsync(string assetId, AssetAvailabilityType availability, bool value);
    }
}
