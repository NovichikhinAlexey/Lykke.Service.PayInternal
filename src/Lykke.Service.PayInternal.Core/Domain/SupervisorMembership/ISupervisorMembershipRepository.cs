using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.SupervisorMembership
{
    public interface ISupervisorMembershipRepository
    {
        Task<ISupervisorMembership> AddAsync(ISupervisorMembership src);
        Task<ISupervisorMembership> GetAsync(string employeeId);
        Task UpdateAsync(ISupervisorMembership src);
        Task RemoveAsync(string employeeId);
    }
}
