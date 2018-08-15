using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.SupervisorMembership
{
    /// <summary>
    /// Supervisor membership details
    /// </summary>
    public class MerchantsSupervisorMembershipResponse
    {
        /// <summary>
        /// Gets or sets employee id
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets merchants to create membership for
        /// </summary>
        public IEnumerable<string> Merchants { get; set; }
    }
}
