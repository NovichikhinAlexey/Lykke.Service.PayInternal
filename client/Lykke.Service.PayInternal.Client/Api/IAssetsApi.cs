using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IAssetsApi
    {
        [Get("/api/assets/settings/general")]
        Task<AvailableAssetsResponse> GetGeneralAvailableAssetsAsync([Query] AssetAvailabilityType type);

        [Get("/api/assets/settings/personal")]
        Task<AvailableAssetsByMerchantResponse> GetPersonalAvailableAssetsAsync([Query] string merchantId);

        [Post("/api/assets/settings/general")]
        Task SetGeneralAvailableAssetsAsync([Body] UpdateAssetAvailabilityRequest request);

        [Post("/api/assets/settings/personal")]
        Task SetPersonalAvailableAssetsAsync([Body] UpdateAssetAvailabilityByMerchantRequest request);
    }
}
