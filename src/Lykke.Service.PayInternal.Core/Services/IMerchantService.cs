using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantService
    {
        Task<IReadOnlyList<IMerchant>> GetAsync();
        
        Task<IMerchant> GetAsync(string merchantId);

        Task<IMerchant> CreateAsync(IMerchant merchant);
        
        Task UpdateAsync(IMerchant merchant);

        Task SetPublicKeyAsync(string merchantId, string publicKey);
        
        Task DeleteAsync(string merchantId);
    }
}
