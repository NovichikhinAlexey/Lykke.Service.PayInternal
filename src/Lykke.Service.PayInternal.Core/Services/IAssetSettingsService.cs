using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetSettingsService
    {
        #region general

        Task<IReadOnlyList<IAssetGeneralSettings>> GetGeneralAsync();

        Task<IReadOnlyList<IAssetGeneralSettings>> GetGeneralAsync(AssetAvailabilityType type);

        Task<IAssetGeneralSettings> SetGeneralAsync(IAssetGeneralSettings availability);

        Task<IAssetGeneralSettings> GetGeneralAsync(string assetId);

        Task<BlockchainType> GetNetworkAsync(string assetId);

        #endregion

        #region merchant

        Task<IAssetMerchantSettings> GetByMerchantAsync(string merchantId);

        Task<IAssetMerchantSettings> SetByMerchantAsync(string merchantId, string paymentAssets, string settlementAssets);

        #endregion

        #region resolving

        Task<IReadOnlyList<string>> ResolveAsync(string merchantId, AssetAvailabilityType type);

        Task<IReadOnlyList<string>> ResolveSettlementAsync(string merchantId);

        Task<IReadOnlyList<string>> ResolvePaymentAsync(string merchantId, string settlementAssetId);

        #endregion
    }
}
