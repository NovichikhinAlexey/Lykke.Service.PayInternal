using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.SupervisorMembership
{
    /// <summary>
    /// Update supervisor membership details
    /// </summary>
    public class UpdateSupervisorMembershipRequest
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
        /// Gets or sets merchant groups for membership in
        /// </summary>
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
