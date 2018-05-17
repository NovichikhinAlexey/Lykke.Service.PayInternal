using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Orders;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrdersRepository : IOrderRepository
    {
        private readonly INoSQLTableStorage<OrderEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByOperation;

        public OrdersRepository(
            [NotNull] INoSQLTableStorage<OrderEntity> storage, 
            [NotNull] INoSQLTableStorage<AzureIndex> indexByOperation)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _indexByOperation = indexByOperation ?? throw new ArgumentNullException(nameof(indexByOperation));
        }

        public async Task<IOrder> GetByPaymentRequestAsync(string paymentRequestId, string orderId)
        {
            OrderEntity entity = await _storage.GetDataAsync(
                OrderEntity.ByPaymentRequest.GeneratePartitionKey(paymentRequestId),
                OrderEntity.ByPaymentRequest.GenerateRowKey(orderId));

            return Mapper.Map<Core.Domain.Orders.Order>(entity);
        }

        public async Task<IReadOnlyList<IOrder>> GetByPaymentRequestAsync(string paymentRequestId)
        {
            string partitionKey = OrderEntity.ByPaymentRequest.GeneratePartitionKey(paymentRequestId);

            IEnumerable<OrderEntity> entities = await _storage.GetDataAsync(partitionKey);

            return Mapper.Map<IEnumerable<Core.Domain.Orders.Order>>(entities).ToList();
        }

        public async Task<IOrder> GetByLykkeOperationAsync(string operationId)
        {
            AzureIndex index = await _indexByOperation.GetDataAsync(
                OrderEntity.IndexByLykkeOperationId.GeneratePartitionKey(operationId),
                OrderEntity.IndexByLykkeOperationId.GenerateRowKey());

            if (index == null) return null;

            OrderEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.Orders.Order>(entity);
        }

        public async Task<IOrder> InsertAsync(IOrder order)
        {
            var entity = OrderEntity.ByPaymentRequest.Create(order);
            
            await _storage.InsertAsync(entity);

            AzureIndex index = OrderEntity.IndexByLykkeOperationId.Create(entity);

            if (index != null)
                await _indexByOperation.InsertAsync(index);

            return Mapper.Map<Core.Domain.Orders.Order>(entity);
        }
    }
}
