using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.MerchantGroups
{
    /// <summary>
    /// Get merchants by usage request details
    /// </summary>
    public class GetMerchantsByUsageModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey(ErrorMessage = "Invalid characters used or value is empty")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets merchant group use
        /// </summary>
        [Required]
        [EnumDataType(typeof(MerchantGroupUse), ErrorMessage = "Invalid value, possible values are: Supervising, Billing")]
        public MerchantGroupUse? MerchantGroupUse { get; set; }
    }
}
