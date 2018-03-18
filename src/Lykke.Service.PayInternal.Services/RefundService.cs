using System;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class RefundService : IRefundService
    {
        private readonly IPaymentRequestService _paymentRequestService;

        public RefundService(IPaymentRequestService paymentRequestService)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
        }

        public async Task<RefundResult> ExecuteAsync(string merchantId, string paymentRequestId, string destinationWalletAddress)
        {
            RefundResult refundResult =
                await _paymentRequestService.RefundAsync(new RefundCommand
                {
                    MerchantId = merchantId,
                    PaymentRequestId = paymentRequestId,
                    DestinationAddress = destinationWalletAddress
                });

            await _paymentRequestService.ProcessAsync(refundResult.PaymentRequestWalletAddress);

            return refundResult;
        }
    }
}
