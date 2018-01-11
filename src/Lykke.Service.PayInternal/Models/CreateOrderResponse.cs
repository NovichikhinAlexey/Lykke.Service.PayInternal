using System;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Models
{
    public class CreateOrderResponse : ICreateOrderResponse
    {
        public string OrderId { get; set; }

        public DateTime DueDate { get; set; }

        public string AssetPairId { get; set; }

        public string InvoiceAssetId { get; set; }

        public double InvoiceAmount { get; set; }

        public string ExchangeAssetId { get; set; }

        public string ExchangeAmount { get; set; }
    }
}
