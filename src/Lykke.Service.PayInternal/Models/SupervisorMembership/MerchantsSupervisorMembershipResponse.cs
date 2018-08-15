using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models.SupervisorMembership
{
    public class MerchantsSupervisorMembershipResponse
    {
        public string EmployeeId { get; set; }

        public string MerchantId { get; set; }

        public IEnumerable<string> Merchants { get; set; }
    }
}
