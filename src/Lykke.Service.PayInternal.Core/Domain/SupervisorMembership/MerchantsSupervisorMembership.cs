using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.SupervisorMembership
{
    public class MerchantsSupervisorMembership : IMerchantsSupervisorMembership
    {
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public IEnumerable<string> Merchants { get; set; }
    }
}
