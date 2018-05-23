using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetsAvailabilityService
    {
        #region general settings

        Task<IReadOnlyList<IAssetAvailability>> GetGeneralByTypeAsync(AssetAvailabilityType type);

        Task<IReadOnlyList<IAssetAvailability>> GetGeneralAsync();

        Task<IAssetAvailability> SetGeneralAsync(IAssetAvailability availability);

        Task<BlockchainType> GetNetworkAsync(string assetId);

        #endregion

        #region personal settings

        Task<IAssetAvailabilityByMerchant> GetPersonalAsync(string merchantId);

        Task<IAssetAvailabilityByMerchant> SetPersonalAsync(string merchantId, string paymentAssets, string settlementAssets);

        #endregion

        #region resolving

        Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type);

        Task<IReadOnlyList<string>> ResolveSettlementAsync(string merchantId);

        Task<IReadOnlyList<string>> ResolvePaymentAsync(string merchantId, string settlementAssetId);

        #endregion
    }
}
