using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IAssetsApi
    {
        [Get("/api/assets/available")]
        Task<AvailableAssetsResponse> GetAvailableAsync([Query] AssetAvailabilityType availabilityType);
        [Get("/api/assets/availablebymerchant")]
        Task<AvailableAssetsResponse> GetAvailableAsync([Query] AssetByMerchantModel assetByMerchant);
        [Get("/api/assets/availablepersonal")]
        Task<AvailableAssetsResponse> GetAvailableAsync([Query] string merchantId);

        [Post("/api/assets/available")]
        Task SetAvailabilityAsync([Body] UpdateAssetAvailabilityRequest request);
        [Post("/api/assets/availablebymerchant")]
        Task SetAvailabilityByMerchantAsync([Body] UpdateAssetAvailabilityByMerchantRequest request);
    }
}
