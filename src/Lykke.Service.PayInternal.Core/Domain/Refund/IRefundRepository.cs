using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Refund
{
    public interface IRefundRepository
    {
        Task AddAsync(IRefund refund);
        Task<IRefund> GetAsync(string merchantId, string refundId);
    }
}
