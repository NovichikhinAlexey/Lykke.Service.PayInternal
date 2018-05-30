using Lykke.Service.PayInternal.Core.Domain.Supervisor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ISupervisorService
    {
        Task<ISupervisor> GetAsync(string employeeId);
        Task<ISupervisor> SetAsync(ISupervisor supervisor);
        Task DeleteAsync(string employeeId);
    }
}
