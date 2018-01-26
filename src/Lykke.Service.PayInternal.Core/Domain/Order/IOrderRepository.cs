using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrderRepository
    {
        Task<IReadOnlyList<IOrder>> GetAsync(string paymentRequestId);
       
        Task<IOrder> InsertAsync(IOrder order);
    }
}
