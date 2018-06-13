using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.MerchantWallets;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IMerchantWalletsApi
    {
        [Post("/api/merchantWallets")]
        Task<MerchantWalletResponse> CreateAsync([Body] CreateMerchantWalletRequest request);

        [Delete("/api/merchantWallets/{merchantWalletId}")]
        Task DeleteAsync(string merchantWalletId);

        [Post("/api/merchantWallets/defaultAssets")]
        Task SetDefaultAssetsAsync([Body] UpdateMerchantWalletDefaultAssetsRequest request);

        [Get("/api/merchantWallets/{merchantId}")]
        Task<IEnumerable<MerchantWalletResponse>> GetByMerchantAsync(string merchantId);

        [Get("/api/merchantWallets/{merchantId}/default")]
        Task<MerchantWalletResponse> GetDefaultAsync(string merchantId, [Query] string assetId, [Query] PaymentDirection paymentDirection);

        [Get("/api/merchantWallets/balances/{merchantId}")]
        Task<IEnumerable<MerchantWalletBalanceResponse>> GetBalancesAsync(string merchantId);
    }
}
