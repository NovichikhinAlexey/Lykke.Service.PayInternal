using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class UpdateAssetAvailabilityByMerchantRequest
    {
        [Required]
        public string PaymentAssets { get; set; }

        [Required]
        public string SettlementAssets { get; set; }

        [Required]
        public string MerchantId { get; set; }
    }
}
