using System;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IOrderService
    {
        Task<IOrder> GetAsync(string paymentRequestId, string orderId);

        Task<IOrder> GetActualAsync(string paymentRequestId, DateTime date);
        
        Task<IOrder> GetLatestOrCreateAsync(IPaymentRequest paymentRequest, bool force = false);

        Task<ICalculatedAmountInfo> GetCalculatedAmountInfoAsync(string settlementAssetId, string paymentAssetId, decimal amount, string merchantId);

        Task<(string AssetPairId, decimal PaymentAmount, decimal Rate)> GetPaymentInfoAsync(string settlementAssetId,
            string paymentAssetId, decimal amount, string merchantId, IRequestMarkup requestMarkup);
    }
}
