using System.ComponentModel.DataAnnotations;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.Orders
{
    public class ChechoutRequestModel
    {
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        [Required]
        [RowKey]
        public string PaymentRequestId { get; set; }

        public bool Force { get; set; }
    }
}
