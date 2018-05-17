using System;
using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Orders;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class OrderEntity : AzureTableEntity
    {
        private decimal _settlementAmount;
        private decimal _paymentAmount;
        private DateTime _dueDate;
        private DateTime _createdDate;
        private decimal? _exchangeRate;
        private DateTime _extendedDueDate;
        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string PaymentRequestId { get; set; }
        public string AssetPairId { get; set; }
        public string LwOperationId { get; set; }
        
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

        public DateTime ExtendedDueDate
        {
            get => _extendedDueDate;
            set
            {
                _extendedDueDate = value;
                MarkValueTypePropertyAsDirty(nameof(ExtendedDueDate));
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

        public decimal? ExchangeRate
        {
            get => _exchangeRate;
            set
            {
                _exchangeRate = value;
                MarkValueTypePropertyAsDirty(nameof(ExchangeRate));
            }
        }

        public static class ByPaymentRequest
        {
            public static string GeneratePartitionKey(string paymentRequestId)
            {
                return paymentRequestId;
            }

            public static string GenerateRowKey(string orderId = null)
            {
                return orderId ?? Guid.NewGuid().ToString("D");
            }

            public static OrderEntity Create(IOrder order)
            {
                var entity = new OrderEntity
                {
                    PartitionKey = GeneratePartitionKey(order.PaymentRequestId),
                    RowKey = GenerateRowKey()
                };

                return Mapper.Map(order, entity);
            }
        }

        public static class IndexByLykkeOperationId
        {
            public static string GeneratePartitionKey(string operationId)
            {
                return operationId;
            }

            public static string GenerateRowKey()
            {
                return "LykkeOperationIndex";
            }

            public static AzureIndex Create(OrderEntity entity)
            {
                return string.IsNullOrEmpty(entity.LwOperationId)
                    ? null
                    : AzureIndex.Create(GeneratePartitionKey(entity.LwOperationId), GenerateRowKey(), entity);
            }
        }
    }
}
