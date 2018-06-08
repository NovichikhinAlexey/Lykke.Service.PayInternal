using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.MerchantGtoups
{
    /// <summary>
    /// Merchant group update details
    /// </summary>
    public class UpdateMerchantGroupModel
    {
        /// <summary>
        /// Gets or sets group id
        /// </summary>
        [Required]
        [RowKey]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets list of merchants
        /// </summary>
        [Required]
        [RowKey]
        [NotEmptyCollection]
        public IEnumerable<string> Merchants { get; set; }

        /// <summary>
        /// Gets or sets merchant group use
        /// </summary>
        [Required]
        [EnumDataType(typeof(MerchantGroupUse), ErrorMessage = "Invalid value, possible values are: Supervising, Billing")]
        public MerchantGroupUse MerchantGroupUse { get; set; }
    }
}
