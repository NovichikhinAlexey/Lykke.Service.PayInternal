using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Supervisor
{
    public interface ISupervisor
    {
        string Id { get; }
        string MerchantId { get; set; }
        string EmployeeId { get; set; }
        string MerchantGroups { get; set; }
        string SupervisorMerchants { get; set; }
    }
}
