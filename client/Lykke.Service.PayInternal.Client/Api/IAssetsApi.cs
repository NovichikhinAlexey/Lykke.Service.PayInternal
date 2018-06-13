using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.AssetRates;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IAssetsApi
    {
        [Get("/api/assets/settings/general")]
        Task<IEnumerable<AssetGeneralSettingsResponse>> GetAssetGeneralSettingsAsync();

        [Post("/api/assets/settings/general")]
        Task SetAssetGeneralSettingsAsync([Body] UpdateAssetGeneralSettingsRequest request);

        [Get("/api/assets/settings/merchant")]
        Task<AssetMerchantSettingsResponse> GetAssetMerchantSettingsAsync([Query] string merchantId);

        [Post("/api/assets/settings/merchant")]
        Task SetAssetMerchantSettingsAsync([Body] UpdateAssetMerchantSettingsRequest settingsRequest);

        [Post("/api/assetRates")]
        Task<AssetRateResponse> AddAssetPairRateAsync([Body] AddAssetRateRequest request);

        [Get("/api/assetRates/{baseAssetId}/{quotingAssetId}")]
        Task<AssetRateResponse> GetCurrentAssetPairRateAsync(string baseAssetId, string quotingAssetId);
    }
}
