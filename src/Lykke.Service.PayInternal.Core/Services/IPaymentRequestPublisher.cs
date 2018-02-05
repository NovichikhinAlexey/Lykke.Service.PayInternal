using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IPaymentRequestPublisher
    {
        Task PublishAsync(IPaymentRequest paymentRequest);
    }
}
