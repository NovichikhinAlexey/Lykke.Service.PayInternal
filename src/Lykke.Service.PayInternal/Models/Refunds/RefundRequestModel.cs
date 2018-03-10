using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.Refunds
{
    public class RefundRequestModel
    {
        [Required]
        public string MerchantId { get; set; }

        [Required]
        public string SourceAddress { get; set; }

        public string DestinationAddress { get; set; }
    }
}
