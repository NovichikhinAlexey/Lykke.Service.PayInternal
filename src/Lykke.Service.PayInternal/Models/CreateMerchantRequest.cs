using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Models
{
    public class CreateMerchantRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ApiKey { get; set; }

        [Required]
        public double DeltaSpread { get; set; }

        [Required]
        public int TimeCacheRates { get; set; }

        [Required]
        public double LpMarkupPercent { get; set; }

        [Required]
        public int LpMarkupPips { get; set; }

        [Required]
        public double MarkupFixedFee { get; set; }
        
        public string LwId { get; set; }
    }
}
