using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IOrderRepository
    {
        Task<IOrder> GetAsync(string paymentRequestId, string orderId);
        
        Task<IReadOnlyList<IOrder>> GetAsync(string paymentRequestId);
       
        Task<IOrder> InsertAsync(IOrder order);
    }
}
