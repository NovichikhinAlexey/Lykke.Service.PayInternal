using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrderEntity : AzureTableEntity, IOrder
    {
        public OrderEntity()
        {
        }

        public OrderEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string AssetPairId { get; set; }
        public double SettlementAmount { get; set; }
        public double PaymentAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }

        internal void Map(IOrder order)
        {
            MerchantId = order.MerchantId;
            PaymentRequestId = order.PaymentRequestId;
            AssetPairId = order.AssetPairId;
            SettlementAmount = order.SettlementAmount;
            PaymentAmount = order.PaymentAmount;
            DueDate = order.DueDate;
            CreatedDate = order.CreatedDate;
        }
    }
}
