using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IPaymentRequestPublisher : IRequestPublisher<IPaymentRequest>
    {
        Task PublishAsync(IPaymentRequest paymentRequest, PaymentRequestRefund refundInfo);
    }
}
