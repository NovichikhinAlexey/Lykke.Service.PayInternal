using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    /// <summary>
    /// New payment request details
    /// </summary>
    public class CreatePaymentRequestModel
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        public string OrderId { get; set; }
        
        /// <summary>
        /// Gets or sets settlement asset id
        /// </summary>
        [Required]
        public string SettlementAssetId{ get; set; }
        
        /// <summary>
        /// Gets or sets payment asset id
        /// </summary>
        [Required]
        public string PaymentAssetId { get; set; }
        
        /// <summary>
        /// Gets or sets due date
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        [DateNotNull(ErrorMessage = "DueDate can't be empty")]
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// Gets or sets markup percent
        /// </summary>
        [Required]
        public double MarkupPercent { get; set; }
        
        /// <summary>
        /// Gets or sets markup pips
        /// </summary>
        [Required]
        public int MarkupPips { get; set; }

        /// <summary>
        /// Gets or sets markup fee
        /// </summary>
        [Required]
        public double MarkupFixedFee { get; set; }

        /// <summary>
        /// Gets ot sets the initiator of payment request creation (PublicAPI, InvoiceService)
        /// </summary>
        [Required]
        public string Initiator { get; set; }
    }
}
