using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Order;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IOrdersApi
    {
        [Get("/api/merchants/paymentrequests/{paymentRequestId}/orders/{orderId}")]
        Task<OrderModel> GetByIdAsync(string paymentRequestId, string orderId);
    }
}
