using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.SupervisorMembership
{
    /// <summary>
    /// Add supervisor membership details
    /// </summary>
    public class AddSupervisorMembershipRequest
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
        /// Gets or sets merchant groups
        /// </summary>
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
