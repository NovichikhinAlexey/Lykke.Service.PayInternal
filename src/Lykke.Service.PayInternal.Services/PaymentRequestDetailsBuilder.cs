using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class PaymentRequestDetailsBuilder : IPaymentRequestDetailsBuilder
    {
        public async Task<PaymentRequestDetails> Build(IPaymentRequest paymentRequest)
        {
            throw new System.NotImplementedException();
        }
    }
}
