using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class RefundService : IRefundService
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ILog _log;

        public RefundService(IPaymentRequestService paymentRequestService, ILog log)
        {
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<RefundResult> ExecuteAsync(string merchantId, string paymentRequestId,
            string destinationWalletAddress)
        {
            RefundResult refundResult;

            try
            {
                refundResult = await _paymentRequestService.RefundAsync(new RefundCommand
                {
                    MerchantId = merchantId,
                    PaymentRequestId = paymentRequestId,
                    DestinationAddress = destinationWalletAddress
                });

                await _paymentRequestService.UpdateStatusAsync(refundResult.PaymentRequestWalletAddress);
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(RefundService), nameof(ExecuteAsync), new
                {
                    merchantId,
                    paymentRequestId,
                    destinationWalletAddress
                }.ToJson(), e);

                if (e is PaymentRequestNotFoundException ||
                    e is NotAllowedStatusException ||
                    e is NoTransactionsToRefundException ||
                    e is MultiTransactionRefundNotSupportedException ||
                    e is OperationFailedException ||
                    e is OperationPartiallyFailedException)
                    throw new RefundException(e.Message);
                
                throw;
            }

            return refundResult;
        }
    }
}
