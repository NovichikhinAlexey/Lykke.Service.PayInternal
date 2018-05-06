using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IMarkupsApi
    {
        [Get("/api/markups/default")]
        Task<IReadOnlyList<MarkupResponse>> GetDefaultsAsync();

        [Get("/api/markups/default/{assetPairId}")]
        Task<MarkupResponse> GetDefaultAsync(string assetPairId);

        [Post("/api/markups/default/{assetPairId}")]
        Task SetDefaultAsync(string assetPairId, [Body] UpdateMarkupRequest request);

        [Get("/api/markups/merchant/{merchantId}")]
        Task<IReadOnlyList<MarkupResponse>> GetForMerchantAsync(string merchantId);

        [Get("/api/markups/merchant/{merchantId}/{assetPairId}")]
        Task<MarkupResponse> GetForMerchantAsync(string merchantId, string assetPairId);

        [Post("/api/markups/merchant/{merchantId}/{assetPairId}")]
        Task SetForMerchantAsync(string merchantId, string assetPairId, [Body] UpdateMarkupRequest request);
    }
}
