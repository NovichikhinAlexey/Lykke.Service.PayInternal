using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.SupervisorMembership
{
    /// <summary>
    /// Supervisor membership details
    /// </summary>
    public class AddSupervisorMembershipMerchantsModel
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
        /// Gets or sets merchants to create membership for
        /// </summary>
        [Required]
        [RowKey]
        [NotEmptyCollection]
        public IEnumerable<string> Merchants { get; set; }
    }
}
