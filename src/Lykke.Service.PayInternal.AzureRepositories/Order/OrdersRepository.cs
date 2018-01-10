using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.AzureRepositories.Order
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly INoSQLTableStorage<OrderEntity> _tableStorage;

        public OrdersRepository(INoSQLTableStorage<OrderEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IEnumerable<IOrder>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IEnumerable<IOrder>> GetByMerchantIdAsync(string merchantId)
        {
            return await _tableStorage.GetDataAsync(OrderEntity.ByMerchant.GeneratePartitionKey(merchantId));
        }

        public async Task SaveAsync(IOrder order)
        {
            var newItem = OrderEntity.ByMerchant.Create(order);

            await _tableStorage.InsertOrMergeAsync(newItem);
        }
    }
}
