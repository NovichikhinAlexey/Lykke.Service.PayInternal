using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class RefundRequestModel
    {
        [Required]
        public string MerchantId { get; set; }

        [Required]
        public string PaymentRequestId { get; set; }

        public string DestinationAddress { get; set; }

        public string CallbackUrl { get; set; }
    }
}
