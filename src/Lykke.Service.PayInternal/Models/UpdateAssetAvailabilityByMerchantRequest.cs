using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.PayInternal.Models
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
