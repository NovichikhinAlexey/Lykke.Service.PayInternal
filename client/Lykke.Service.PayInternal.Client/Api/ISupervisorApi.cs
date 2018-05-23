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
        [Get("/api/supervising/{merchantId}/{employeeId}")]
        Task<SupervisingMerchantsResponse> GetSupervisingMerchantsAsync([Query] string merchantId, string employeeId);

        [Delete("/api/supervising/{merchantId}/{employeeId}")]
        Task DeleteSupervisingMerchantsAsync([Query] string merchantId, string employeeId);

        [Post("/api/supervising")]
        Task SetSupervisingMerchantsAsync([Body] CreateSupervisingEmployeeRequest request);
    }
}
