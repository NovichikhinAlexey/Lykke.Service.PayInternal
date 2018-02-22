using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models
{
    public class BiAddressAmount : IBiAddressAmount
    {
        /// <summary>
        /// Source address
        /// </summary>
        [Required]
        public string SourceAddress { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        [Required]
        public string DestinationAddress { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        [Required]
        public decimal Amount { get; set; }
    }
}
