using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Refund;

namespace Lykke.Service.PayInternal.Models.Refunds
{
    public class RefundRequestModel : IRefundRequest
    {
        [Required]
        public string MerchantId { get; set; }

        [Required]
        public string SourceAddress { get; set; }

        [Required]
        public string DestinationAddress { get; set; }
    }
}
