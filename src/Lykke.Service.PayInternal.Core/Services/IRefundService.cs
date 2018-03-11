using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Refund;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IRefundService
    {
        Task<IRefund> ExecuteAsync(IRefundRequest refund);
        Task<IRefund> GetStateAsync(string merchantId, string refundId);
    }
}
