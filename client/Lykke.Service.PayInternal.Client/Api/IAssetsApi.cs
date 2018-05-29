using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
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
    }
}
