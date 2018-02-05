using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IOrderService
    {
        Task<IOrder> GetAsync(string paymentRequestId, string orderId);

        Task<IOrder> GetAsync(string paymentRequestId, DateTime date);
        
        Task<IOrder> GetLatestOrCreateAsync(IPaymentRequest paymentRequest);

        Task<PaymentRequestStatusInfo> GetPaymentStatus(IReadOnlyList<IBlockchainTransaction> transactions,
            string paymentRequestId);
    }
}
