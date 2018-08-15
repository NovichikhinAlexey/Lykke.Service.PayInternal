using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class UpdateAssetMerchantSettingsRequest
    {
        [Required]
        public string PaymentAssets { get; set; }

        [Required]
        public string SettlementAssets { get; set; }

        [Required]
        [MerchantExists]
        public string MerchantId { get; set; }
    }
}
