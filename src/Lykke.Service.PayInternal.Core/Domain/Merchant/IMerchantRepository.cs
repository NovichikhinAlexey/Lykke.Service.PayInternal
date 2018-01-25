using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface IMerchantRepository
    {
        Task<IMerchant> GetAsync(string id);

        Task<IEnumerable<IMerchant>> GetAsync();

        Task<IMerchant> SaveAsync(IMerchant merchant);
    }
}
