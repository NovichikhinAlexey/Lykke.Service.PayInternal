using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Order;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IOrdersApi
    {
        [Get("/api/merchants/paymentrequests/{paymentRequestId}/orders/{orderId}")]
        Task<OrderModel> GetByIdAsync(string paymentRequestId, string orderId);

        [Post("/api/orders")]
        Task<OrderModel> ChechoutAsync([Body] ChechoutRequestModel model);

        [Post("/api/orders/calculate")]
        Task<CalculatedAmountResponse> GetCalculatedAmountInfoAsync([Body] GetCalculatedAmountInfoRequest model);
    }
}
