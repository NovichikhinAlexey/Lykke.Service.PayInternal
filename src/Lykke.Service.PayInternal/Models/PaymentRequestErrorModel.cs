using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Models
{
    public class PaymentRequestErrorModel
    {
        public CreatePaymentRequestErrorType Code { get; set; }
    }
}
