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
        public decimal SettlementAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal PaymentAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the order due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the order extended due date
        /// </summary>
        public DateTime ExtendedDueDate { get; set; }
        
        /// <summary>
        /// Gets or sets the order created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the order echange rate
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Gts or sets lykke wallet operation id
        /// </summary>
        public string LwOperationId { get; set; }
    }
}
