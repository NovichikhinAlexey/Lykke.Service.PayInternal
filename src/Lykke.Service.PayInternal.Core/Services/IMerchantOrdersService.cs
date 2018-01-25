using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantOrdersService
    {
        Task<IOrder> CreateOrder(ICreateOrder request);
        Task<IOrder> ReCreateOrder(IReCreateOrder request);
    }
}
