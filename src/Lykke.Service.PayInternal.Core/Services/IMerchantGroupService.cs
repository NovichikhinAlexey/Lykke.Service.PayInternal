using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Groups;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantGroupService
    {
        Task<IMerchantGroup> CreateAsync(IMerchantGroup src);

        Task<IMerchantGroup> GetAsync(string id);

        Task UpdateAsync(IMerchantGroup src);

        Task DeleteAsync(string id);

        Task<IReadOnlyList<string>> GetMerchantsByUsageAsync(string merchantId, MerchantGroupUse merchantGroupUse);

        Task<IReadOnlyList<IMerchantGroup>> GetByOwnerAsync(string ownerId);
    }
}
