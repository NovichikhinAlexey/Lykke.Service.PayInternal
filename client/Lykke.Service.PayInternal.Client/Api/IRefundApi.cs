using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Refunds;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IRefundApi
    {
        [Post("/api/refunds/Refund")]
        Task<RefundResponse> CreateRefundRequestAsync(string paymentRequestId, string walletAddress = null);

        [Get("/api/refunds/GetRefund")]
        Task<RefundResponse> GetRefundAsync(string refundId);
    }
}
