using System;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    /// <summary>
    /// Represent a payment request.
    /// </summary>
    public class PaymentRequestModel
    {
        /// <summary>
        /// Gets or sets the payment request id.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the merchant id.
        /// </summary>
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Get or set ExternalOrderId
        /// </summary>
        public string ExternalOrderId { get; set; }

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
        public decimal MarkupPercent { get; set; }
        
        /// <summary>
        /// Gets or sets the markup pips.
        /// </summary>
        public int MarkupPips { get; set; }

        /// <summary>
        /// Gets or sets markup fixed fee
        /// </summary>
        public decimal MarkupFixedFee { get; set; }
        
        /// <summary>
        /// Gets or sets the wallet address.
        /// </summary>
        public string WalletAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the payment request status.
        /// </summary>
        public PaymentRequestStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the paid amount.
        /// </summary>
        public decimal PaidAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the paid date.
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the error occurred during processing payment request.
        /// </summary>
        public PaymentRequestProcessingError ProcessingError { get; set; }

        /// <summary>
        /// Gets ot sets the initiator of payment request creation (PublicAPI, InvoiceService)
        /// </summary>
        public string Initiator { get; set; }
    }
}
