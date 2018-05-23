using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Supervisor
{
    public class Supervisor : ISupervisor
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public string MerchantGroups { get; set; }
        public string SupervisorMerchants { get; set; }
    }
}
