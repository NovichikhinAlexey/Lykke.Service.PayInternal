using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.SupervisorMembership
{
    public class SupervisorMembership : ISupervisorMembership
    {
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
