using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class OrderEntity : AzureTableEntity, IOrder
    {
        private decimal _settlementAmount;
        private decimal _paymentAmount;
        private DateTime _dueDate;
        private DateTime _createdDate;

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
        
        public decimal SettlementAmount
        {
            get => _settlementAmount;
            set
            {
                _settlementAmount = value;
                MarkValueTypePropertyAsDirty(nameof(SettlementAmount));
            }
        }
        
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                MarkValueTypePropertyAsDirty(nameof(PaymentAmount));
            }
        }
        
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
            }
        }
        
        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                MarkValueTypePropertyAsDirty(nameof(CreatedDate));
            }
        }

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
