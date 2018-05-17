using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Orders
{
    public interface IOrderRepository
    {
        Task<IOrder> GetByPaymentRequestAsync(string paymentRequestId, string orderId);
        
        Task<IReadOnlyList<IOrder>> GetByPaymentRequestAsync(string paymentRequestId);

        Task<IOrder> GetByLykkeOperationAsync(string operationId);
       
        Task<IOrder> InsertAsync(IOrder order);
    }
}
