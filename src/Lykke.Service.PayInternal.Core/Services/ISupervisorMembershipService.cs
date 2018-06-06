using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ISupervisorMembershipService
    {
        Task<ISupervisorMembership> AddAsync(ISupervisorMembership src);
        Task<IMerchantsSupervisorMembership> AddAsync(IMerchantsSupervisorMembership src);
        Task<ISupervisorMembership> GetAsync(string employeeId);
        Task<IMerchantsSupervisorMembership> GetWithMerchantsAsync(string employeeId);
        Task UpdateAsync(ISupervisorMembership src);
        Task RemoveAsync(string employeeId);
    }
}
