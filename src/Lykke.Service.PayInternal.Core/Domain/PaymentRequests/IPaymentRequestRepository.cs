using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public interface IPaymentRequestRepository
    {
        Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId);

        Task<bool> HasAnyPaymentRequestAsync(string merchantId);

        Task<IReadOnlyList<IPaymentRequest>> GetByFilterAsync(PaymentsFilter paymentsFilter);

        Task<IPaymentRequest> FindAsync(string walletAddress);

        Task<IReadOnlyList<IPaymentRequest>> GetByDueDate(DateTime from, DateTime to);

        Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId);

        Task<IPaymentRequest> InsertAsync(IPaymentRequest paymentRequest);
        
        Task UpdateAsync(IPaymentRequest paymentRequest);
    }
}
