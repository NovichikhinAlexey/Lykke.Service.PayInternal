using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Core
{
    public static class PaymentRequestExtensions
    {
        public static PaymentRequestStatusInfo GetCurrentStatusInfo(this IPaymentRequest src)
        {
            return new PaymentRequestStatusInfo
            {
                Amount = src.Amount,
                Date = src.PaidDate,
                Status = src.Status,
                ProcessingError = src.ProcessingError
            };
        }
    }
}
