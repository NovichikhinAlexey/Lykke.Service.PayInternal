using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models.SupervisorMembership
{
    public class SupervisorMembershipResponse
    {
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
