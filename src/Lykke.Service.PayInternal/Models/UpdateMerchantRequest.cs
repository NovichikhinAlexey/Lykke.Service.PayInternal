using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateMerchantRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string ApiKey { get; set; }

        [Required]
        public int TimeCacheRates { get; set; }
        
        public string LwId { get; set; }
    }
}
