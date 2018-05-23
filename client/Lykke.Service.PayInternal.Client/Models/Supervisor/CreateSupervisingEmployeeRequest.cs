using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Supervisor
{
    public class CreateSupervisingEmployeeRequest
    {
        public string EmployeeId { get; set; }
        public string MerchantId { get; set; }
        public string MerchantGroups { get; set; }
        public string SupervisorMerchants { get; set; }
    }
}
