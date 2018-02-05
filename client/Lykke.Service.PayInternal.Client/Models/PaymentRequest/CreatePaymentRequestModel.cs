using System;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Represent a payment request creation information.
    /// </summary>
    public class CreatePaymentRequestModel
    {
        /// <summary>
        /// Gets or sets the merchant id.
        /// </summary>
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Gets or sets the external order id
        /// </summary>
        public string OrderId { get; set; }
        
        /// <summary>
        /// Gets or sets the settlement asset id.
        /// </summary>
        public string SettlementAssetId { get; set; }
        
        /// <summary>
        /// Gets or sets the payment asset id.
        /// </summary>
        public string PaymentAssetId { get; set; }
        
        /// <summary>
        /// Gets or sets the payment request due date.
        /// </summary>
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// Gets or sets the markup percent.
        /// </summary>
        public double MarkupPercent { get; set; }
        
        /// <summary>
        /// Gets or sets the markup pips.
        /// </summary>
        public int MarkupPips { get; set; }

        /// <summary>
        /// Gets or sets markup fixed fee
        /// </summary>
        public double MarkupFixedFee { get; set; }
    }
}
