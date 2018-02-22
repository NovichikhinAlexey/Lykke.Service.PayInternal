using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IAssetsApi
    {
        [Get("/api/assets/available")]
        Task<AvailableAssetsResponse> GetAvailableAsync([Query] AssetAvailabilityType availabilityType);

        [Post("/api/assets/available")]
        Task SetAvailabilityAsync([Body] UpdateAssetAvailabilityRequest request);
    }
}
