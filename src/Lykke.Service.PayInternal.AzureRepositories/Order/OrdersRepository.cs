using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrdersRepository : IOrderRepository
    {
        private readonly INoSQLTableStorage<OrderEntity> _storage;

        public OrdersRepository(INoSQLTableStorage<OrderEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IOrder> GetAsync(string paymentRequestId, string orderId)
        {
            return await _storage.GetDataAsync(GetPartitionKey(paymentRequestId), GetRowKey(orderId));
        }

        public async Task<IReadOnlyList<IOrder>> GetAsync(string paymentRequestId)
        {
            IEnumerable<OrderEntity> entities = await _storage.GetDataAsync(GetPartitionKey(paymentRequestId));

            return entities.ToList();
        }

        public async Task<IOrder> InsertAsync(IOrder order)
        {
            var entity = new OrderEntity(GetPartitionKey(order.PaymentRequestId), GetRowKey());
            entity.Map(order);
            
            await _storage.InsertAsync(entity);

            return entity;
        }
        
        private static string GetPartitionKey(string paymentRequestId)
            => paymentRequestId;

        private static string GetRowKey(string orderId)
            => orderId;

        private static string GetRowKey()
            => Guid.NewGuid().ToString("D");
    }
}
