using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.SupervisorMembership
{
    public interface IMerchantsSupervisorMembership
    {
        string MerchantId { get; set; }
        string EmployeeId { get; set; }
        IEnumerable<string> Merchants { get; set; }
    }
}
