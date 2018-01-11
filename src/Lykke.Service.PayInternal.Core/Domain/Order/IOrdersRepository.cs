using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrdersRepository
    {
        Task<IOrder> SaveAsync(IOrder order);

        Task<IEnumerable<IOrder>> GetAsync();

        Task<IEnumerable<IOrder>> GetByMerchantIdAsync(string merchantId);
    }
}
