using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.SupervisorMembership
{
    public interface ISupervisorMembership
    {
        string MerchantId { get; set; }
        string EmployeeId { get; set; }
        IEnumerable<string> MerchantGroups { get; set; }
    }
}
