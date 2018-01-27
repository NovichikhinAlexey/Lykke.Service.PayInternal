using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Order;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IOrdersApi
    {
        [Get("/api/merchants/{merchantId}/paymentrequests/{paymentRequestId}/orders/active")]
        Task<OrderModel> GetAsync(string merchantId, string paymentRequestId);
    }
}
