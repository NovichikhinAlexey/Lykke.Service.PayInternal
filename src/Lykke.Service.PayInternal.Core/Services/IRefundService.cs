using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IRefundService
    {
        Task<RefundResult> ExecuteAsync(string merchantId, string paymentRequestId, string destinationWalletAddress);
    }
}
