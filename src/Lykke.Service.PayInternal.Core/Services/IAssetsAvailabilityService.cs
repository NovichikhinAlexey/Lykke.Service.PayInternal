using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetsAvailabilityService
    {
        Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type);

        Task<IReadOnlyList<string>> ResolveSettlementAsync(string merchantId);

        Task<IReadOnlyList<string>> ResolvePaymentAsync(string merchantId, string settlementAssetId);

        Task<IAssetAvailabilityByMerchant> GetPersonalAsync(string merchantId);

        Task<IAssetAvailabilityByMerchant> SetPersonalAsync(string merchantId, string paymentAssets, string settlementAssets);

        Task<IReadOnlyList<IAssetAvailability>> GetGeneralByTypeAsync(AssetAvailabilityType type);

        Task<IAssetAvailability> SetGeneralAsync(string assetId, AssetAvailabilityType type, bool value);
    }
}
