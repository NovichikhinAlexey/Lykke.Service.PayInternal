using Refit;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.SupervisorMembership;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface ISupervisorMembershipApi
    {
        [Post("/api/supervisorMembership/merchantGroups")]
        Task<SupervisorMembershipResponse> AddAsync([Body] AddSupervisorMembershipRequest request);

        [Get("/api/supervisorMembership/merchantGroups/{employeeId}")]
        Task<SupervisorMembershipResponse> GetAsync(string employeeId);

        [Put("/api/supervisorMembership/merchantGroups")]
        Task UpdateAsync([Body] UpdateSupervisorMembershipRequest request);

        [Delete("/api/supervisorMembership/{employeeId}")]
        Task RemoveAsync(string employeeId);

        [Post("/api/supervisorMembership/merchants")]
        Task<MerchantsSupervisorMembershipResponse> AddForMerchantsAsync([Body] AddSupervisorMembershipMerchantsRequest request);

        [Get("/api/supervisorMembership/merchants/{employeeId}")]
        Task<MerchantsSupervisorMembershipResponse> GetWithMerchantsAsync(string employeeId);
    }
}
