using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetMerchantSettingsRepository
    {
        Task<IAssetMerchantSettings> GetAsync(string merchantId);

        Task<IAssetMerchantSettings> SetAsync(string paymentAssets, string settlementAssets, string merchantId);
    }
}
