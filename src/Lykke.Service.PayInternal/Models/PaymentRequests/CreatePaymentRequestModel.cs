using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    public class CreatePaymentRequestModel
    {
        [Required]
        [RowKey]
        public string MerchantId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }

        public string OrderId { get; set; }
        
        [Required]
        public string SettlementAssetId{ get; set; }
        
        [Required]
        public string PaymentAssetId { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        [DateNotNull(ErrorMessage = "DueDate can't be empty")]
        public DateTime DueDate { get; set; }
        
        [Required]
        public double MarkupPercent { get; set; }
        
        [Required]
        public int MarkupPips { get; set; }

        [Required]
        public double MarkupFixedFee { get; set; }
    }
}
