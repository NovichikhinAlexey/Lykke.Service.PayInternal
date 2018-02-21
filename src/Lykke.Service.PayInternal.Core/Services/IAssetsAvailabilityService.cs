using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetsAvailabilityService
    {
        Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(AssetAvailabilityType assetAvailability);

        Task<IAssetAvailability> UpdateAsync(string assetId, AssetAvailabilityType assetAvailability, bool value);
    }
}
