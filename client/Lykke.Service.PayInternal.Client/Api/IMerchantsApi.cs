using Lykke.Service.PayInternal.Client.Models.Merchant;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IMerchantsApi
    {
        [Get("/api/merchants")]
        Task<IReadOnlyList<MerchantModel>> GetAllAsync();
        
        [Get("/api/merchants/{merchantId}")]
        Task<MerchantModel> GetByIdAsync(string merchantId);
        
        [Post("/api/merchants")]
        Task<MerchantModel> CreateAsync([Body] CreateMerchantRequest request);
        
        [Patch("/api/merchants")]
        Task UpdateAsync([Body] UpdateMerchantRequest request);
        
        [Multipart]
        [Post("/api/merchants/{merchantId}/publickey")]
        Task SetPublicKeyAsync(string merchantId, [AliasAs("file")] StreamPart stream);
        
        [Delete("/api/merchants/{merchantId}")]
        Task DeleteAsync(string merchantId);

        [Get("/api/merchants/{merchantId}/assets")]
        Task<AvailableAssetsResponse> GetAvailableAssetsAsync(string merchantId, [Query] AssetAvailabilityType type);

        [Get("/api/merchants/{merchantId}/settlementAssets")]
        Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId);

        [Get("/api/merchants/{merchantId}/paymentAssets")]
        Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, [Query] string settlementAssetId);
    }
}
