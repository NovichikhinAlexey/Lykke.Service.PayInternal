using Lykke.Service.PayInternal.Client.Models.Merchant;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IMerchantsApi
    {
        #region merchants

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

        [Get("/api/merchants/{merchantId}/markups/{assetPairId}")]
        Task<MarkupResponse> ResolveMarkupAsync(string merchantId, string assetPairId);

        #endregion

        #region merchant groups

        [Post("/api/merchantGroups")]
        Task<MerchantGroupResponse> AddGroupAsync([Body] AddMerchantGroupRequest request);

        [Get("/api/merchantGroups/{id}")]
        Task<MerchantGroupResponse> GetGroupAsync(string id);

        [Put("/api/merchantGroups")]
        Task UpdateGroupAsync([Body] UpdateMerchantGroupRequest request);

        [Delete("/api/merchantGroups/{id}")]
        Task DeleteGroupAsync(string id);

        [Post("/api/merchantGroups/merchants/byUsage")]
        Task<MerchantsByUsageResponse> GetMerchantsByUsageAsync([Body] GetMerchantsByUsageRequest request);

        [Get("/api/merchantGroups/byOwner/{ownerId}")]
        Task<IEnumerable<MerchantGroupResponse>> GetGroupsByOwnerAsync(string ownerId);

        #endregion
    }
}
