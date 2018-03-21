using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetPersonalAvailabilityRepository
    {
        Task<IAssetAvailabilityByMerchant> GetAsync(string merchantId);

        Task<IAssetAvailabilityByMerchant> SetAsync(string paymentAssets, string settlementAssets, string merchantId);
    }
}
