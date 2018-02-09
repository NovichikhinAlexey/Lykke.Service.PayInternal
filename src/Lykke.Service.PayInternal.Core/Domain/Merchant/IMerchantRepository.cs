using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface IMerchantRepository
    {
        Task<IReadOnlyList<IMerchant>> GetAsync();
        
        Task<IMerchant> GetAsync(string merchantId);

        Task<IMerchant> InsertAsync(IMerchant merchant);
        
        Task ReplaceAsync(IMerchant merchant);
        
        Task DeleteAsync(string merchantId);
    }
}
