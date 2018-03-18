using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IPaymentRequestService
    {
        Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId);
        
        Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId);

        Task<IPaymentRequest> FindAsync(string walletAddress);

        Task<IPaymentRequest> CreateAsync(IPaymentRequest paymentRequest);
        
        Task<IPaymentRequest> CheckoutAsync(string merchantId, string paymentRequestId);

        Task ProcessAsync(string walletAddress);

        Task ProcessByTransactionAsync(string transactionId);

        Task<RefundResult> RefundAsync(string merchantId, string paymentRequestId, string destinationWalletAddress);
    }
}
