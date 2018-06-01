using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Supervisor
{
    public interface ISupervisorRepository
    {
        Task<ISupervisor> GetAsync(string employeeId);
        Task<ISupervisor> InsertAsync(ISupervisor supervisor);
        Task UpdateAsync(ISupervisor supervisor);
        Task DeleteAsync(string employeeId);
    }
}
