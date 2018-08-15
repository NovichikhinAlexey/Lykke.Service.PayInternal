using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Orders
{
    public class GetCalculatedAmountInfoRequest
    {
        [AssetExists]
        public string SettlementAssetId { get; set; }
        [AssetExists]
        public string PaymentAssetId { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Must be greater zero")]
        public decimal Amount { get; set; }
        [MerchantExists]
        public string MerchantId { get; set; }
    }
}
