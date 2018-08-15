using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.SupervisorMembership
{
    /// <summary>
    /// Supervisor membership details
    /// </summary>
    public class AddSupervisorMembershipModel
    {
        /// <summary>
        /// Gets or sets employee id
        /// </summary>
        [Required]
        [RowKey]
        public string EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets merchant groups for membership in
        /// </summary>
        [Required]
        [RowKey]
        [NotEmptyCollection]
        public IEnumerable<string> MerchantGroups { get; set; }
    }
}
