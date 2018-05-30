using Lykke.Service.PayInternal.Client.Models.Supervisor;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Client.Api
{
    public interface ISupervisorApi
    {
        [Get("/api/supervising/{employeeId}")]
        Task<SupervisingMerchantsResponse> GetSupervisingMerchantsAsync(string employeeId);

        [Delete("/api/supervising/{employeeId}")]
        Task DeleteSupervisingMerchantsAsync(string employeeId);

        [Post("/api/supervising")]
        Task SetSupervisingMerchantsAsync([Body] CreateSupervisingEmployeeRequest request);
    }
}
