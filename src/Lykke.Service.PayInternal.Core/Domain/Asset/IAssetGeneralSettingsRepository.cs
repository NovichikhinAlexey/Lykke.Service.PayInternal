using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetGeneralSettingsRepository
    {
        Task<IReadOnlyList<IAssetGeneralSettings>> GetAsync();

        Task<IReadOnlyList<IAssetGeneralSettings>> GetAsync(AssetAvailabilityType availabilityType);

        Task<IAssetGeneralSettings> GetAsync(string assetId);

        Task<IAssetGeneralSettings> SetAsync(IAssetGeneralSettings availability);
    }
}
