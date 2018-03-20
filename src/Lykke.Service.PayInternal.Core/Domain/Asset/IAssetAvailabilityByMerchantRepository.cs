using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetAvailabilityByMerchantRepository
    {
        Task<IAssetAvailabilityByMerchant> GetAsync(string merchantId);
        Task<IAssetAvailabilityByMerchant> SetAsync(string paymentAssets, string settlementAssets, string merchantId);
    }
}
