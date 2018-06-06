using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.SupervisorMembership
{
    /// <summary>
    /// Supervisor membership update details
    /// </summary>
    public class UpdateSupervisorMembershipModel
    {
        /// <summary>
        /// Gets or sets employee id
        /// </summary>
        [Required]
        public string EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets merchant groups for membership in
        /// </summary>
        [Required]
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
