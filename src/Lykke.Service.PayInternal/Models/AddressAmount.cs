using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models
{
    public class AddressAmount : IAddressAmount
    {
        /// <summary>
        /// Address (source/destination)
        /// </summary>
        [Required]
        public string Address { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        [Required]
        public decimal Amount { get; set; }
    }
}
