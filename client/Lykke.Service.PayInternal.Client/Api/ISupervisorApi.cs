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
        Task<SupervisingMerchantsResponse> GetSupervisingAsync(string employeeId);

        [Delete("/api/supervising/{employeeId}")]
        Task DeleteSupervisingAsync(string employeeId);

        [Post("/api/supervising")]
        Task SetSupervisingAsync([Body] CreateSupervisingEmployeeRequest request);
    }
}
