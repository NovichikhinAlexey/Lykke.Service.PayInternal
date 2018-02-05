using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class CreatePaymentRequestModel
    {
        [Required]
        public string MerchantId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string SettlementAssetId{ get; set; }
        
        [Required]
        public string PaymentAssetId { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Required]
        public double MarkupPercent { get; set; }
        
        [Required]
        public int MarkupPips { get; set; }
    }
}
