using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Refunds;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IRefundApi
    {
        [Post("/api/refund")]
        Task<RefundResponse> CreateRefundRequestAsync([Body] RefundRequestModel request);

        [Get("/api/refund/{merchantId}/{refundId}")]
        Task<RefundResponse> GetRefundAsync(string merchantId, string refundId);
    }
}
