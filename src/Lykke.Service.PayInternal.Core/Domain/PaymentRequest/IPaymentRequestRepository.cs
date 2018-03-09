using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequest
{
    public interface IPaymentRequestRepository
    {
        Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId);

        Task<IPaymentRequest> FindAsync(string walletAddress);

        Task<IEnumerable<IPaymentRequest>> GetNotExpiredAsync();
        
        Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId);

        Task<IPaymentRequest> InsertAsync(IPaymentRequest paymentRequest);
        
        Task UpdateAsync(IPaymentRequest paymentRequest);
    }
}
