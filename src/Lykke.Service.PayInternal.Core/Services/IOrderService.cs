using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IOrderService
    {
        Task<IOrder> GetAsync(string merchantId, string paymentRequestId);
    }
}
