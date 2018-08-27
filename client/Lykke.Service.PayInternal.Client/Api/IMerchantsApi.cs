using Refit;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.Markup;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IMerchantsApi
    {
        [Get("/api/merchants/{merchantId}/assets")]
        Task<AvailableAssetsResponse> GetAvailableAssetsAsync(string merchantId, [Query] AssetAvailabilityType type);

        [Get("/api/merchants/{merchantId}/settlementAssets")]
        Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId);

        [Get("/api/merchants/{merchantId}/paymentAssets")]
        Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, [Query] string settlementAssetId);

        [Get("/api/merchants/{merchantId}/markups/{assetPairId}")]
        Task<MarkupResponse> ResolveMarkupAsync(string merchantId, string assetPairId);
    }
}
