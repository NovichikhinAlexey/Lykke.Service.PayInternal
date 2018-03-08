using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetsAvailabilityService
    {
        Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(AssetAvailabilityType assetAvailability);

        Task<IAssetAvailability> UpdateAsync(string assetId, AssetAvailabilityType assetAvailability, bool value);
        Task<IAssetAvailabilityByMerchant> UpdateMerchantAssetsAsync(string paymentAssets, string settlementAssets, string merchantId);
        Task<IReadOnlyList<IAssetAvailability>> GetAvailableAsync(string merchantId, AssetAvailabilityType assetAvailabilityType);
        Task<IReadOnlyList<IAssetAvailability>> GetAssetsAvailabilityFromSettings(AssetAvailabilityType assetAvailability);
    }
}
