using System;

namespace Lykke.Service.PayInternal.Client.Models.Order
{
    /// <summary>
    /// Represent an payment request order.
    /// </summary>
    public class OrderModel
    {
        /// <summary>
        /// Gets or sets the order id.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the merchant id.
        /// </summary>
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Gets or sets the payment request id.
        /// </summary>
        public string PaymentRequestId { get; set; }
        
        /// <summary>
        /// Gets or sets the asset pair id.
        /// </summary>
        public string AssetPairId { get; set; }
        
        /// <summary>
        /// Gets or sets the settlement amount.
        /// </summary>
        public double SettlementAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public double PaymentAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the order due date.
        /// </summary>
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// Gets or sets the order created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
