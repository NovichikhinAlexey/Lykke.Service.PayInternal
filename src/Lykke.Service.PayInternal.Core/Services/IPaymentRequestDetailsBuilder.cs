using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IPaymentRequestDetailsBuilder
    {
        Task<TResult> Build<TResult, TOrder, TTransaction, TRefund>(IPaymentRequest paymentRequest, PaymentRequestRefund refundInfo);
    }
}
